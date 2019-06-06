// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-10
// Last Modified:			2019-06-06
// 

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation
{
    public class NavigationViewComponent : ViewComponent
    {
        public NavigationViewComponent(
            NavigationTreeBuilderService siteMapTreeBuilder,
            IEnumerable<INavigationNodePermissionResolver> permissionResolvers,
            IEnumerable<IFindCurrentNode> nodeFinders,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccesor,
            INodeUrlPrefixProvider prefixProvider,
            ILogger<NavigationViewComponent> logger)
        {
            _builder = siteMapTreeBuilder;
            _permissionResolvers = permissionResolvers;
            _nodeFinders = nodeFinders;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccesor = actionContextAccesor;
            _prefixProvider = prefixProvider;
            
            _log = logger;
        }

        private ILogger _log;
        private NavigationTreeBuilderService _builder;
        private IEnumerable<INavigationNodePermissionResolver> _permissionResolvers;
        private IEnumerable<IFindCurrentNode> _nodeFinders;
        private IUrlHelperFactory _urlHelperFactory;
        private IActionContextAccessor _actionContextAccesor;
        private INodeUrlPrefixProvider _prefixProvider;

        

        public async Task<IViewComponentResult> InvokeAsync(string viewName, string filterName, string startingNodeKey)
        {
            var rootNode = await _builder.GetTree();
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccesor.ActionContext);
            NavigationViewModel model = new NavigationViewModel(
                startingNodeKey,
                filterName,
                Request.HttpContext,
                urlHelper,
                rootNode,
                _permissionResolvers,
                _nodeFinders,
                _prefixProvider.GetPrefix(),
                _log);

            return View(viewName, model);
        }



    }
}
