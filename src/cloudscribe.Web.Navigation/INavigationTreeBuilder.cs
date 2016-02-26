// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-09
// Last Modified:			2016-02-26
// 

using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation
{
    public interface INavigationTreeBuilder
    {
        string Name { get; }
        Task<TreeNode<NavigationNode>> BuildTree(NavigationTreeBuilderService service);
    }
}
