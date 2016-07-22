// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-09
// Last Modified:			2016-07-22
// 

using Newtonsoft.Json;
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
        [DefaultValue(false)]
        public bool IsRootNode { get; set; } = false;
        

        #endregion

        //#region INavigationNodeLocalization

        //[DefaultValue("")]
        //public string ResourceName { get; set; } = string.Empty;

        //[DefaultValue("")]
        //public string ResourceTextKey { get; set; } = string.Empty;

        //[DefaultValue("")]
        //public string ResourceTitleKey { get; set; } = string.Empty;

        //#endregion

        #region INavigationNodeRenderSettings

        //[DefaultValue(true)]
        //public bool IncludeAmbientValuesInUrl { get; set; } = true;

        [DefaultValue("")]
        public string PreservedRouteParameters { get; set; } = string.Empty;

        [DefaultValue("")]
        public string ComponentVisibility { get; set; } = string.Empty;

        [DefaultValue("")]
        public string ViewRoles { get; set; } = string.Empty;

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
