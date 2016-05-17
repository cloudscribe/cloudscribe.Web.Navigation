// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2016-02-26
// Last Modified:			2016-05-17
// 

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public class MemoryTreeCache : ITreeCache
    {
        public MemoryTreeCache(
            IMemoryCache cache,
            IOptions<TreeCacheOptions> optionsAccessor = null)
        {
            if (cache == null) { throw new ArgumentNullException(nameof(cache)); }
            this.cache = cache;

            options = optionsAccessor?.Value;
            if (options == null) options = new TreeCacheOptions(); //default
        }

        private IMemoryCache cache;

        private TreeCacheOptions options;

        public Task<TreeNode<NavigationNode>> GetTree(string cacheKey)
        {
            TreeNode<NavigationNode> result = (TreeNode<NavigationNode>)cache.Get(cacheKey);

            return Task.FromResult(result);
        }

        public void AddToCache(TreeNode<NavigationNode> tree, string cacheKey)
        {
            cache.Set(
                cacheKey,
                tree,
                new MemoryCacheEntryOptions()
                 .SetSlidingExpiration(TimeSpan.FromSeconds(options.CacheDurationInSeconds))
                 );
        }
    }
}
