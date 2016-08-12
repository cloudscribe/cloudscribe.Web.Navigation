// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-07
// Last Modified:			2016-08-12
// 

using Newtonsoft.Json.Linq;
using System;

namespace cloudscribe.Web.Navigation
{
    public class NavigationTreeJsonConverter : JsonCreationConverter<TreeNode<NavigationNode>>
    {
        public string ConvertToJsonIndented(TreeNode<NavigationNode> tNode)
        {
            return tNode.ToJsonIndented();
        }

        public string ConvertToJsonCompact(TreeNode<NavigationNode> tNode)
        {
            return tNode.ToJsonCompact();
        }

        protected override TreeNode<NavigationNode> Create(Type objectType, JObject jObject)
        {
            if(jObject["Value"] == null) { return null; }

            TreeNode<NavigationNode> treeRoot = CreateTreeNode(null, jObject);
            if(jObject["Children"] != null)
            {
                JArray firstLevelChildren = (JArray)jObject["Children"];
                AddChildren(treeRoot, firstLevelChildren);
            }
            
        
            return treeRoot;
        }

        private TreeNode<NavigationNode> CreateTreeNode(TreeNode<NavigationNode> tNode, JToken jNode)
        {
            //build the child node
            NavigationNode navNode = new NavigationNode();

            if(jNode["Value"]["Key"] != null)
            {
                navNode.Key = (string)jNode["Value"]["Key"];
            }
            
            if(jNode["Value"]["ParentKey"] != null)
            {
                navNode.ParentKey = (string)jNode["Value"]["ParentKey"];
            }
            
            if(jNode["Value"]["Controller"] != null)
            {
                navNode.Controller = (string)jNode["Value"]["Controller"];
            }
            
            if(jNode["Value"]["Action"] != null)
            {
                navNode.Action = (string)jNode["Value"]["Action"];
            }

            if (jNode["Value"]["Area"] != null)
            {
                navNode.Area = (string)jNode["Value"]["Area"];
            }

            if (jNode["Value"]["NamedRoute"] != null)
            {
                navNode.NamedRoute = (string)jNode["Value"]["NamedRoute"];
            }

            if (jNode["Value"]["Text"] != null)
            {
                navNode.Text = (string)jNode["Value"]["Text"];
            }

            if (jNode["Value"]["Title"] != null)
            {
                navNode.Title = (string)jNode["Value"]["Title"];
            }

            if (jNode["Value"]["Url"] != null)
            {
                navNode.Url = (string)jNode["Value"]["Url"];
            }
            else
            {
                navNode.Url = navNode.ResolveUrl();
            }

            

            if (jNode["Value"]["PreservedRouteParameters"] != null)
            {
                navNode.PreservedRouteParameters = (string)jNode["Value"]["PreservedRouteParameters"];
            }

            if (jNode["Value"]["ComponentVisibility"] != null)
            {
                navNode.ComponentVisibility = (string)jNode["Value"]["ComponentVisibility"];
            }

            if (jNode["Value"]["ViewRoles"] != null)
            {
                navNode.ViewRoles = (string)jNode["Value"]["ViewRoles"];
            }

            if (jNode["Value"]["CustomData"] != null)
            {
                navNode.CustomData = (string)jNode["Value"]["CustomData"];
            }

            if (jNode["Value"]["IsRootNode"] != null)
            {
                navNode.IsRootNode = Convert.ToBoolean((string)jNode["Value"]["IsRootNode"]);
            }

            if (jNode["Value"]["HideFromAuthenticated"] != null)
            {
                navNode.HideFromAuthenticated = Convert.ToBoolean((string)jNode["Value"]["HideFromAuthenticated"]);
            }

            if (jNode["Value"]["HideFromAnonymous"] != null)
            {
                navNode.HideFromAnonymous = Convert.ToBoolean((string)jNode["Value"]["HideFromAnonymous"]);
            }

            if (jNode["Value"]["IsClickable"] != null)
            {
                navNode.IsClickable = Convert.ToBoolean((string)jNode["Value"]["IsClickable"]);
            }

            if (jNode["Value"]["IconCssClass"] != null)
            {
                navNode.IconCssClass = (string)jNode["Value"]["IconCssClass"];
            }

            if (jNode["Value"]["CssClass"] != null)
            {
                navNode.CssClass = (string)jNode["Value"]["CssClass"];
            }

            //TODO: add DataAttributes collection
            

            if (tNode == null)
            {
                TreeNode<NavigationNode> rootNode = new TreeNode<NavigationNode>(navNode);
                return rootNode;
            }
            else
            { 
                TreeNode<NavigationNode> childNode = tNode.AddChild(navNode);

                return childNode;
            }

            
        }

        private void AddChildren(TreeNode<NavigationNode> node, JArray children)
        {
            foreach (var child in children)
            {
                TreeNode<NavigationNode> childNodeT = CreateTreeNode(node, child);
                
                if(child["Children"] != null)
                {
                    JArray subChildren = (JArray)child["Children"];
                    if (subChildren.Count > 0)
                    {
                        // recursion
                        AddChildren(childNodeT, subChildren);
                    }

                }
                
            }

        }
        

    }
}
