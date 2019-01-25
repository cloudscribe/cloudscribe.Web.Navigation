// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-09
// Last Modified:			2019-01-25
// 

using System;
using System.Collections.Generic;

namespace cloudscribe.Web.Navigation
{
    public interface INavigationNode
    {
        string Key { get; set; }
        string ParentKey { get; set; }
        string Text { get; set; }
        string Title { get; set; }
        string Url { get; set; } 
        string Controller { get; set; } 
        string Action { get; set; }
        string Area { get; set; }
        string Page { get; set; }

        string NamedRoute { get; set; }

        //[Obsolete("This property should not be used and will be removed in future versions. This property cannot be relied on to identitify the root of the tree, instead use the .IsRoot() method on TreeNode which determines the root by a null parent.")]
        //bool IsRootNode { get; set; }
    }
    
    public interface INavigationNodeRenderSettings
    {
        
        string PreservedRouteParameters { get; set; }
        string ComponentVisibility { get; set; }
        string ViewRoles { get; set; }
        string CustomData { get; set; }
        bool ChildContainerOnly { get; set; }
        bool HideFromAuthenticated { get; set; }
        bool HideFromAnonymous { get; set; }
    }
    
    public interface INavigationNodeDesignMeta
    {
        
        bool IsClickable { get; set; }
        string IconCssClass { get; set; }
        string CssClass { get; set; }
        string MenuDescription { get; set; }
        string Target { get; set; }

        List<DataAttribute> DataAttributes { get; set; }
    }


}
