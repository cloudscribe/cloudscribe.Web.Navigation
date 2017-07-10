// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-10
// Last Modified:			2016-09-01
// 

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using cloudscribe.Web.Navigation.Helpers;
using Microsoft.AspNetCore.Mvc.Routing;

namespace cloudscribe.Web.Navigation
{
    public class NavigationViewModel
    {
        public NavigationViewModel(
            string startingNodeKey,
            string navigationFilterName,
            HttpContext context,
            IUrlHelper urlHelper,
            TreeNode<NavigationNode> rootNode,
            IEnumerable<INavigationNodePermissionResolver> permissionResolvers,
            IEnumerable<IFindCurrentNode> nodeFinders,
            string nodeSearchUrlPrefix,
            ILogger logger)
        {
            this.navigationFilterName = navigationFilterName;
            this.nodeSearchUrlPrefix = nodeSearchUrlPrefix;
            this.context = context;
            this.RootNode = rootNode;
            this.permissionResolvers = permissionResolvers;
            this.nodeFinders = nodeFinders;
            this.urlHelper = urlHelper;
            this.startingNodeKey = startingNodeKey;
            log = logger;

            removalFilters.Add(FilterIsAllowed);
            foreach(var permissionResolver in permissionResolvers)
            {
                removalFilters.Add(permissionResolver.ShouldAllowView);
            }
            
            removalFilters.Add(IsAllowedByAdjuster);



        }

        private ILogger log;
        private string startingNodeKey;
        private string navigationFilterName;
        private string nodeSearchUrlPrefix;
        private HttpContext context;
        private IUrlHelper urlHelper;
        private IEnumerable<INavigationNodePermissionResolver> permissionResolvers;
        private IEnumerable<IFindCurrentNode> nodeFinders;
        private List<Func<TreeNode<NavigationNode>, bool>> removalFilters = new List<Func<TreeNode<NavigationNode>, bool>>();

        public TreeNode<NavigationNode> RootNode { get; private set; }

        /// <summary>
        /// a place to temporarily stash a node for processing by a template
        /// </summary>
        public TreeNode<NavigationNode> TempNode { get; private set; } = null;

        private TreeNode<NavigationNode> startingNode = null;

        public TreeNode<NavigationNode> StartingNode
        {
            // lazy load
            get
            {
                if (startingNode == null)
                {
                    if (startingNodeKey.Length > 0)
                    {
                        startingNode = RootNode.FindByKey(startingNodeKey);
                        if (startingNode == null)
                        {
                            log.LogWarning("could not find navigation node for starting node key "
                                + startingNodeKey
                                + " will fallback to RootNode.");
                        }
                    }

                    return RootNode;
                }

                return startingNode;
            }
        }

        private TreeNode<NavigationNode> currentNode = null;
        /// <summary>
        /// the node corresponding to the current request url
        /// </summary>
        public TreeNode<NavigationNode> CurrentNode
        {
            // lazy load
            get
            {
                if (currentNode == null)
                {
                    if (RootNode == null) return null;
                    //log.LogInformation("currentNode was null so lazy loading it");
                    currentNode = RootNode.FindByUrl(urlHelper, context.Request.Path, nodeSearchUrlPrefix);

                    if (currentNode == null)
                    {
                        if (startingNodeKey.Length > 0)
                        {
                            return StartingNode;
                        }
                        // a way to plug in custom logic to find the current node if the default approach does not
                        foreach(var finder in nodeFinders)
                        {
                            var result = finder.FindNode(RootNode, urlHelper, context.Request.Path, nodeSearchUrlPrefix);
                            if(result != null)
                            {
                                currentNode = result;
                                break;
                            }
                        }
                    }
                        
                    
                }
                return currentNode;
            }
        }

        public TreeNode<NavigationNode> ParentNode
        {
            // lazy load
            get
            {
                if (CurrentNode.Parent != null)
                {
                    return CurrentNode.Parent;
                }
                return null;
            }
        }

        private List<TreeNode<NavigationNode>> parentChain = null;
        public List<TreeNode<NavigationNode>> ParentChain
        {
            // lazy load
            get {
                if (parentChain == null)
                {
                    var includeCurrentNode = ShouldAllowView(CurrentNode);
                    parentChain = CurrentNode.GetParentNodeChain(includeCurrentNode, true);
                }
                return parentChain;
            }
        }

        private List<NavigationNode> tailCrumbs = null;
        public List<NavigationNode> TailCrumbs
        {
            get
            {
                if(tailCrumbs == null)
                {
                    if (context.Items[Constants.TailCrumbsContexctKey] != null)
                        tailCrumbs = (List<NavigationNode>)context.Items[Constants.TailCrumbsContexctKey];
                }
                return tailCrumbs;
            }
        }

        public string AdjustText(TreeNode<NavigationNode> node)
        {
            string key = NavigationNodeAdjuster.KeyPrefix + node.Value.Key;
            if (context.Items[key] != null)
            {
                NavigationNodeAdjuster adjuster = (NavigationNodeAdjuster)context.Items[key];
                if(adjuster.ViewFilterName == navigationFilterName)
                {
                    if (adjuster.AdjustedText.Length > 0) { return adjuster.AdjustedText; }
                }
                
            }

            return node.Value.Text;
        }

        public string AdjustUrl(TreeNode<NavigationNode> node)
        {
            string urlToUse = string.Empty;
            try
            {
                if ((node.Value.Action.Length > 0) && (node.Value.Controller.Length > 0))
                {
                    if (node.Value.PreservedRouteParameters.Length > 0)
                    {
                        List<string> preservedParams = node.Value.PreservedRouteParameters.SplitOnChar(',');
                        //var queryBuilder = new QueryBuilder();
                        //var routeParams = new { };
                        var queryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (string p in preservedParams)
                        {
                            if (context.Request.Query.ContainsKey(p))
                            {
                                queryStrings.Add(p, context.Request.Query[p]);
                            }
                        }

                        urlToUse = urlHelper.Action(node.Value.Action, node.Value.Controller, new { area = node.Value.Area });
                        
                        if ((urlToUse != null) && (queryStrings.Count > 0))
                        {
                            urlToUse = QueryHelpers.AddQueryString(urlToUse, queryStrings);
                        }

                    }
                    else
                    {  
                        urlToUse = urlHelper.Action(node.Value.Action, node.Value.Controller, new { area = node.Value.Area });                
                    }

                }
                else if (node.Value.NamedRoute.Length > 0)
                {
                    urlToUse = urlHelper.RouteUrl(node.Value.NamedRoute);
                }

                string key = NavigationNodeAdjuster.KeyPrefix + node.Value.Key;

                if (context.Items[key] != null)
                {
                    NavigationNodeAdjuster adjuster = (NavigationNodeAdjuster)context.Items[key];
                    if (adjuster.ViewFilterName == navigationFilterName)
                    {
                        if (adjuster.AdjustedUrl.Length > 0) { return adjuster.AdjustedUrl; }
                    }
                }

                if (string.IsNullOrEmpty(urlToUse)) { return node.Value.Url; }
            }
            catch(ArgumentOutOfRangeException ex)
            {
                log.LogError("error handled for " + node.Value.Key, ex);
            }
            

            //if(urlToUse.Length > 0) { return urlToUse; }
       
            return urlToUse;
        }
        
        public bool ShouldAllowView(TreeNode<NavigationNode> node)
        {
            if (node.Value.HideFromAnonymous && !context.User.Identity.IsAuthenticated) { return false; } //this line should be handled in NavigationNodePermissionResolver
            if (node.Value.HideFromAuthenticated && context.User.Identity.IsAuthenticated) { return false; }

            foreach (var filter in removalFilters)
            {
                if (!filter.Invoke(node)) { return false; }
            }

            return true;
        }

        public bool HasVisibleChildren(TreeNode<NavigationNode> node)
        {
            if(node == null) { return false; }

            foreach(var childNode in node.Children)
            {
                if(ShouldAllowView(childNode)) { return true; }
            }

            return false;
        }

        public string UpdateTempNode(TreeNode<NavigationNode> node)
        {
            TempNode = node;

            return string.Empty;
        }

        private bool IsAllowedByAdjuster(TreeNode<NavigationNode> node)
        {
            string key = NavigationNodeAdjuster.KeyPrefix + node.Value.Key;
            if (context.Items[key] != null)
            {
                NavigationNodeAdjuster adjuster = (NavigationNodeAdjuster)context.Items[key];
                if (adjuster.ViewFilterName == navigationFilterName)
                {
                    if(adjuster.AdjustRemove) { return false; }
                }
            }

            return true;
        }

        private bool FilterIsAllowed(TreeNode<NavigationNode> node)
        {
            if (string.IsNullOrEmpty(node.Value.ComponentVisibility)) { return true; }
            if (navigationFilterName.Length == 0) { return false; }
            if (node.Value.ComponentVisibility.Contains(navigationFilterName)) { return true; }
           
            return false;
        }




    }
}
