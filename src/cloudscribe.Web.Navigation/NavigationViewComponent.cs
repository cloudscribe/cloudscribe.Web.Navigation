// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-10
// Last Modified:			2016-02-26
// 

using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation
{
    public class NavigationViewComponent : ViewComponent
    {
        public NavigationViewComponent(
            NavigationTreeBuilderService siteMapTreeBuilder,
            INavigationNodePermissionResolver permissionResolver,
            IUrlHelper urlHelper,
            INodeUrlPrefixProvider prefixProvider,
            ILogger<NavigationViewComponent> logger)
        {
            builder = siteMapTreeBuilder;
            this.permissionResolver = permissionResolver;
            this.urlHelper = urlHelper;
            if(prefixProvider == null)
            {
                this.prefixProvider = new DefaultNodeUrlPrefixProvider();
            }
            else
            {
                this.prefixProvider = prefixProvider;
            }
            log = logger;
        }

        private ILogger log;
        private NavigationTreeBuilderService builder;
        private INavigationNodePermissionResolver permissionResolver;
        private IUrlHelper urlHelper;
        private INodeUrlPrefixProvider prefixProvider;

        public async Task<IViewComponentResult> InvokeAsync(string viewName, string filterName)
        {
            return await InvokeAsync(viewName, filterName, string.Empty);
            
        }

        public async Task<IViewComponentResult> InvokeAsync(string viewName, string filterName, string startingNodeKey)
        {
            TreeNode<NavigationNode> rootNode = await builder.GetTree();

            NavigationViewModel model = new NavigationViewModel(
                startingNodeKey,
                filterName,
                Request.HttpContext,
                urlHelper,
                rootNode,
                permissionResolver,
                prefixProvider.GetPrefix(),
                log);

            return View(viewName, model);
        }



    }
}
