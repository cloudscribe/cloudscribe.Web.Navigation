// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-10-12
// Last Modified:			2016-02-26
// 

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public class DistributedTreeCache : ITreeCache
    {
        public DistributedTreeCache(
            IDistributedCache cache,
            IOptions<TreeCacheOptions> optionsAccessor = null)
        {
            if (cache == null) { throw new ArgumentNullException(nameof(cache)); }
            this.cache = cache;

            options = optionsAccessor?.Value;
            if (options == null) options = new TreeCacheOptions(); //default
        }

        private IDistributedCache cache;
        private TreeCacheOptions options;

        public Task<TreeNode<NavigationNode>> GetTree(string cacheKey)
        {
            throw new NotImplementedException();

            //TreeNode<NavigationNode> result = null;
            //await cache.ConnectAsync();
            //byte[] bytes = await cache.GetAsync(cacheKey);
            //if (bytes != null)
            //{
            //    // TODO how to deserialize from the bytes?

            //    //log.LogDebug("rootnode was found in distributed cache so deserializing");
            //    //string xml = Encoding.UTF8.GetString(bytes);
            //    //XDocument doc = XDocument.Parse(xml);

            //    //rootNode = converter.FromXml(doc);
            //}
            
            //return result;
        }

        public void AddToCache(TreeNode<NavigationNode> tree, string cacheKey)
        {
            throw new NotImplementedException();

            //TODO: how best to serialize to bytes

            //string xml2 = converter.ToXmlString(tree);

            //await cache.SetAsync(
            //                    cacheKey,
            //                    Encoding.UTF8.GetBytes(xml2),
            //                    new DistributedCacheEntryOptions().SetSlidingExpiration(
            //                        TimeSpan.FromSeconds(options.CacheDurationInSeconds))
            //                        );
        }

    }
}
