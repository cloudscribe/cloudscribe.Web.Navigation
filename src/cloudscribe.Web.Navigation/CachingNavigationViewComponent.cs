// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using cloudscribe.Web.Navigation.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation
{
    public class CachingNavigationViewComponent : ViewComponent
    {
        public CachingNavigationViewComponent(
            NavigationTreeBuilderService siteMapTreeBuilder,
            IEnumerable<INavigationNodePermissionResolver> permissionResolvers,
            IEnumerable<IFindCurrentNode> nodeFinders,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccesor,
            INodeUrlPrefixProvider prefixProvider,
            ILogger<NavigationViewComponent> logger,
            IDOMTreeCache DomCache,
            IRazorViewEngine viewEngine,
            NavViewRenderer viewRenderer,
            ITempDataProvider tempDataProvider)
        {
            _builder              = siteMapTreeBuilder;
            _permissionResolvers  = permissionResolvers;
            _nodeFinders          = nodeFinders;
            _urlHelperFactory     = urlHelperFactory;
            _actionContextAccesor = actionContextAccesor;
            _prefixProvider       = prefixProvider;
            _log                  = logger;
            _domCache             = DomCache;
            _viewEngine           = viewEngine;
            _viewRenderer         = viewRenderer;
            _tempDataProvider     = tempDataProvider;
        }

        private ILogger                                        _log;
        private readonly IDOMTreeCache                         _domCache;
        private readonly IRazorViewEngine                      _viewEngine;
        private readonly NavViewRenderer                       _viewRenderer;
        private readonly ITempDataProvider                     _tempDataProvider;
        private NavigationTreeBuilderService                   _builder;
        private IEnumerable<INavigationNodePermissionResolver> _permissionResolvers;
        private IEnumerable<IFindCurrentNode>                  _nodeFinders;
        private IUrlHelperFactory                              _urlHelperFactory;
        private IActionContextAccessor                         _actionContextAccesor;
        private INodeUrlPrefixProvider                         _prefixProvider;


        // intention here is not to re-compute the whole navigation DOM tree repeatedly
        // when you get a large number of unauthenticated page requests e.g. from a PWA
        // The main problem is clearing this cache again on new page creation etc 
        // since .Net memory cache has no method for enumerating its keys - 
        // you need to know the specific key name.
        // In a simplecontent system I'd probably need to use the IHandlePageCreated (etc) 
        // hooks to clear a navcache of known name.

        public async Task<IViewComponentResult> InvokeAsync(string viewName, 
                                                            string filterName, 
                                                            string startingNodeKey, 
                                                            int    expirationSeconds = 60,
                                                            bool   testMode = false)
        {
            NavigationViewModel model = null;

            string cacheKey = $"{viewName}_{filterName}_{startingNodeKey}";

            // authenticated users - always do what the stadard version of this component does:
            // build the tree afresh
            if (User.Identity.IsAuthenticated)
            {
                // maybe kill cache key here under certain circumstances?
                // if(User.IsInRole("Administrators") || User.IsInRole("Content Administrators"))
                // {
                //    // await _domCache.ClearDOMTreeCache(cacheKey);
                // }

                model = await CreateNavigationTree(filterName, startingNodeKey);
                return View(viewName, model);
            }
            else
            {
                var result = await _domCache.GetDOMTree(cacheKey); // use the viewname as the key in the cache

                if (string.IsNullOrEmpty(result))
                {
                    model = await CreateNavigationTree(filterName, startingNodeKey);

                    ViewEngineResult viewResult = null;
                    var actionContext           = _actionContextAccesor.ActionContext;
                    var tempData                = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

                    string fullViewName = $"Components/CachingNavigation/{viewName}";
                    try
                    {
                        // beware the 'IFeatureCollection has been disposed' System.ObjectDisposedException error here
                        viewResult = _viewEngine.FindView(actionContext, fullViewName, true);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, $"CachingNavigationViewComponent: Failed to search for View {fullViewName}");
                    }

                    if (viewResult == null || !viewResult.Success || viewResult.View == null)
                    {
                        _log.LogError($"CachingNavigationViewComponent: Failed to find a matching view {fullViewName}");
                    }
                    else
                    {
                        try
                        {
                            result = await _viewRenderer.RenderViewAsStringWithActionContext(fullViewName,
                                                                        model,
                                                                        viewResult,
                                                                        actionContext,
                                                                        tempData
                                                                        );

                            if (!string.IsNullOrEmpty(result)) 
                            {
                                if(testMode)
                                {
                                    await _domCache.StoreDOMTree(cacheKey, $"<h2>Cached copy from {cacheKey}</h2> {result}", expirationSeconds);
                                }
                                else 
                                {
                                    await _domCache.StoreDOMTree(cacheKey, result, expirationSeconds);
                                }
                            }

                            _log.LogInformation($"CachingNavigationViewComponent: Rendered view successfully for {fullViewName}");
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, $"CachingNavigationViewComponent: Failed to render view for {fullViewName}");
                            throw (ex);
                        }
                    }
                }
                return View("CachedNav", result);
            }
        }

        /// <summary>
        /// The expensive thing...
        /// </summary>
        private async Task<NavigationViewModel> CreateNavigationTree(string filterName, string startingNodeKey)
        {
            var rootNode  = await _builder.GetTree();
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
            return model;
        }
    }
}
