// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-09
// Last Modified:			2017-06-20
// 

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace cloudscribe.Web.Navigation
{
    public class NavigationNode : INavigationNode, INavigationNodeRenderSettings, INavigationNodeDesignMeta
    {
        public NavigationNode()
        {
            DataAttributes = new List<DataAttribute>();
        }

        #region INavigationNode

        [JsonRequired]
        public string Key { get; set; } = string.Empty;

        [Obsolete("ParentKey is obsolete and was never actually used, parent child relationships are established by the xml or json or other treebuilders which add nodes to the navigation tree. That is to say the tree structure itself is the parent child relationship.")]
        [DefaultValue("")]
        public string ParentKey { get; set; } = string.Empty;

        [DefaultValue("")]
        public string Text { get; set; } = string.Empty;

        [DefaultValue("")]
        public string Title { get; set; } = string.Empty;

        [DefaultValue("")]
        public string Url { get; set; } = string.Empty;

        [DefaultValue("")]
        public string Controller { get; set; } = string.Empty;

        [DefaultValue("")]
        public string Action { get; set; } = string.Empty;

        [DefaultValue("")]
        public string Area { get; set; } = string.Empty;

        [DefaultValue("")]
        public string NamedRoute { get; set; } = string.Empty;

        /// <summary>
        /// this property can lie, it is not enforced in creating a tree
        /// a node could start out as a root node and then be added as a sub node of another node
        /// not sure we even need this property 
        /// </summary>
        [Obsolete("This property should not be used and will be removed in future versions. This property cannot be relied on to identitify the root of the tree, instead use the .IsRoot() method on TreeNode which determines the root by a null parent.")]
        [DefaultValue(false)]
        public bool IsRootNode { get; set; } = false;

        public bool ExcludeFromSearchSiteMap { get; set; } = false;
        

        #endregion

        

        #region INavigationNodeRenderSettings
        
        [DefaultValue("")]
        public string PreservedRouteParameters { get; set; } = string.Empty;

        [DefaultValue("")]
        public string ComponentVisibility { get; set; } = string.Empty;

        [DefaultValue("")]
        public string AuthorizationPolicy { get; set; } = string.Empty;

        [DefaultValue("")]
        public string ViewRoles { get; set; } = string.Empty;

        [DefaultValue("")]
        public string CustomData { get; set; } = string.Empty;

        /// <summary>
        /// set to true if the root node itself is not intended to be rendered
        /// false is for the root page to be the "home" page and everything else hangs off it
        /// though the menu templates can make it look like home page is on
        /// the same level as first level child pages
        /// </summary>
        public bool ChildContainerOnly { get; set; } = false;

        public bool HideFromAuthenticated { get; set; } = false;

        public bool HideFromAnonymous { get; set; } = false;

        #endregion

        #region INavigationNodeDesignMeta

        public bool IsClickable { get; set; } = true;

        public string IconCssClass { get; set; } = string.Empty;

        public string CssClass { get; set; } = string.Empty;

        public string MenuDescription { get; set; } = string.Empty;

        public string Target { get; set; } = string.Empty;

        public List<DataAttribute> DataAttributes { get; set; }

        #endregion

    }
}
