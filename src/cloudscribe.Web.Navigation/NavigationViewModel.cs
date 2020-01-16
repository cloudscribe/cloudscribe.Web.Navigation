// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-10
// Last Modified:			2019-01-25
// 

using cloudscribe.Web.Navigation.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private readonly ILogger log;
        private string startingNodeKey;
        private string navigationFilterName;
        private string nodeSearchUrlPrefix;
        private HttpContext context;
        private IUrlHelper urlHelper;
        private IEnumerable<INavigationNodePermissionResolver> permissionResolvers;
        private IEnumerable<IFindCurrentNode> nodeFinders;
        private List<Func<TreeNode<NavigationNode>, Task<bool>>> removalFilters = new List<Func<TreeNode<NavigationNode>, Task<bool>>>();

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
                    if(startingNodeKey == "RootNode")
                    {
                        return RootNode;
                    }

                    if (!string.IsNullOrWhiteSpace(startingNodeKey))
                    {
                        startingNode = RootNode.FindByKey(startingNodeKey);
                        if (startingNode == null)
                        {
                            log.LogWarning($"Could not find navigation node for starting node key '{startingNodeKey}', will fallback to {nameof(RootNode)}.");
                            return RootNode;
                        }
                        else
                        {
                            return startingNode;
                        }
                    }
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
                        if (!string.IsNullOrWhiteSpace(startingNodeKey))
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
                    //2019-07-31 changed this for async but can't change this method to async, not sure if this breaks anything
                    //var includeCurrentNode = ShouldAllowView(CurrentNode);
                    //parentChain = CurrentNode.GetParentNodeChain(includeCurrentNode, true);

                    var includeCurrentNode = true;
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
                    if (!string.IsNullOrWhiteSpace(adjuster.AdjustedText)) { return adjuster.AdjustedText; }
                }
                
            }

            return node.Value.Text;
        }

        public string AdjustUrl(TreeNode<NavigationNode> node)
        {
            string urlToUse = string.Empty;
            try
            {
                var routeValues = new Dictionary<string, object>();
                routeValues.Add("area", node.Value.Area);
                if (!string.IsNullOrWhiteSpace(node.Value.PreservedRouteParameters))
                {
                    List<string> preservedParams = node.Value.PreservedRouteParameters.SplitOnChar(',');
                    foreach (string p in preservedParams)
                    {
                        if (urlHelper.ActionContext.RouteData.Values.ContainsKey(p))
                        {
                            routeValues.Add(p, urlHelper.ActionContext.RouteData.Values[p]);
                        }
                        else if (context.Request.Query.ContainsKey(p))
                        {
                            routeValues.Add(p, context.Request.Query[p]);
                        }
                    }
                }

                if ((!string.IsNullOrWhiteSpace(node.Value.Action)) && (!string.IsNullOrWhiteSpace(node.Value.Controller)))
                {
                    urlToUse = urlHelper.Action(node.Value.Action, node.Value.Controller, routeValues);
                }
                else if (!string.IsNullOrWhiteSpace(node.Value.NamedRoute))
                {
                    urlToUse = urlHelper.RouteUrl(node.Value.NamedRoute, routeValues);
                }
                else if(!string.IsNullOrWhiteSpace(node.Value.Page))
                {
                    urlToUse = urlHelper.Page(node.Value.Page, routeValues);
                }

                string key = NavigationNodeAdjuster.KeyPrefix + node.Value.Key;

                if (context.Items[key] != null)
                {
                    NavigationNodeAdjuster adjuster = (NavigationNodeAdjuster)context.Items[key];
                    if (adjuster.ViewFilterName == navigationFilterName)
                    {
                        if (!string.IsNullOrWhiteSpace(adjuster.AdjustedUrl)) { return adjuster.AdjustedUrl; }
                    }
                }

                if (string.IsNullOrEmpty(urlToUse)) { return node.Value.Url; }
            }
            catch(ArgumentOutOfRangeException ex)
            {
                log.LogError("error handled for " + node.Value.Key, ex);
            }
            

            
       
            return urlToUse;
        }
        
        public async Task<bool> ShouldAllowView(TreeNode<NavigationNode> node)
        {
            if (node.Value.HideFromAnonymous && !context.User.Identity.IsAuthenticated) { return false; }
            if (node.Value.HideFromAuthenticated && context.User.Identity.IsAuthenticated) { return false; }

            foreach (var filter in removalFilters)
            {
                if (! await filter.Invoke(node)) { return false; }
            }

            return true;
        }

        public async Task<bool> HasVisibleChildren(TreeNode<NavigationNode> node)
        {
            if(node == null) { return false; }

            foreach(var childNode in node.Children)
            {
                if(await ShouldAllowView(childNode)) { return true; }
            }

            return false;
        }

        public string UpdateTempNode(TreeNode<NavigationNode> node)
        {
            TempNode = node;

            return string.Empty;
        }

        private Task<bool> IsAllowedByAdjuster(TreeNode<NavigationNode> node)
        {
            string key = NavigationNodeAdjuster.KeyPrefix + node.Value.Key;
            if (context.Items[key] != null)
            {
                NavigationNodeAdjuster adjuster = (NavigationNodeAdjuster)context.Items[key];
                if (adjuster.ViewFilterName == navigationFilterName)
                {
                    if(adjuster.AdjustRemove) { return Task.FromResult(false); }
                }
            }

            return Task.FromResult(true);
        }

        private Task<bool> FilterIsAllowed(TreeNode<NavigationNode> node)
        {
            if (string.IsNullOrEmpty(node.Value.ComponentVisibility)) { return Task.FromResult(true); }
            if (string.IsNullOrWhiteSpace(navigationFilterName)) { return Task.FromResult(false); }
            if (node.Value.ComponentVisibility.Contains(navigationFilterName)) { return Task.FromResult(true); }
           
            return Task.FromResult(false);
        }




    }
}
