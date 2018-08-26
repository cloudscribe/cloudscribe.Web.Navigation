// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:                  Joe Audette
// Created:                 2016-04-19
// Last Modified:           2018-08-26
// 


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace cloudscribe.Web.SiteMap.Controllers
{
    /// <summary>
    /// TODO: this is not implemented to handle huge sites, if the total of all nodes added will be greater 
    /// than 25000 then a sitemap index should be provided that links to multiple sitemaps with each site map
    /// being limted to 25000 urls or less
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SiteMapController : Controller
    {

        public SiteMapController(
            ILogger<SiteMapController> logger,
            IEnumerable<ISiteMapNodeService> nodeProviders = null
            )
        {
            _log = logger;
            _nodeProviders = nodeProviders; 
        }

        private ILogger _log;
        private IEnumerable<ISiteMapNodeService> _nodeProviders;

        [HttpGet]
        [ResponseCache(CacheProfileName = "SiteMapCacheProfile")]
        [Route("api/sitemap")]
        public virtual async Task<IActionResult> Index()
        {
            if (_nodeProviders == null)
            {
                _log.LogInformation("no ISiteMapNodeService were injected so returning 404");
                Response.StatusCode = 404;
                return new EmptyResult();
            }

            CancellationToken cancellationToken = HttpContext?.RequestAborted ?? CancellationToken.None;

            XNamespace xmlns = SiteMapConstants.Namespace;
            var root = new XElement(xmlns + SiteMapConstants.UrlSetTag);
            
            foreach (var nodeService in _nodeProviders)
            {
                var nodeList = await nodeService.GetSiteMapNodes(cancellationToken);
                foreach(var node in nodeList)
                {
                    var url = TryEnsureFullUrl(node.Url);
                    var sitemapElement = new XElement(
                    xmlns + SiteMapConstants.UrlTag,
                    new XElement(xmlns + SiteMapConstants.LocTag, url),

                    node.LastModified == null ? null : new XElement(
                        xmlns + SiteMapConstants.LastModTag,
                        node.LastModified.Value.ToLocalTime().ToString(SiteMapConstants.DateFormat)),

                    node.ChangeFrequency == null ? null : new XElement(
                        xmlns + SiteMapConstants.ChangeFreqTag,
                        node.ChangeFrequency.Value.ToString().ToLowerInvariant()),

                    node.Priority == null ? null : new XElement(
                        xmlns + SiteMapConstants.PriorityTag,
                        node.Priority.Value.ToString(SiteMapConstants.PriorityFormat, CultureInfo.InvariantCulture)));
                    
                    ;

                    root.Add(sitemapElement);
                }
            }

            var xml = new XDocument(root);
            return new XmlResult(xml);

        }

        protected string TryEnsureFullUrl(string providedUrl)
        {
            var isFull = Uri.IsWellFormedUriString(providedUrl, UriKind.Absolute);
            if (isFull) return providedUrl;
            if(providedUrl.StartsWith("/"))
            {
                return $"{Request.Scheme}://{Request.Host}{providedUrl}";
            }
            // unexpected format, just return the provided url
            return providedUrl;
        }

    }
}
