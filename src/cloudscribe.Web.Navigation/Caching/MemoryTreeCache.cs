// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2016-02-26
// Last Modified:			2019-02-15
// 

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public class MemoryTreeCache : ITreeCache
    {
        public MemoryTreeCache(
            IMemoryCache cache,
            IOptions<TreeCacheOptions> optionsAccessor = null)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = optionsAccessor?.Value;
            if (_options == null) _options = new TreeCacheOptions(); //default
        }

        private readonly IMemoryCache _cache;
        private readonly TreeCacheOptions _options;

        public Task<TreeNode<NavigationNode>> GetTree(string cacheKey)
        {
            TreeNode<NavigationNode> result = (TreeNode<NavigationNode>)_cache.Get(cacheKey);

            return Task.FromResult(result);
        }

        public Task AddToCache(TreeNode<NavigationNode> tree, string cacheKey)
        {
            _cache.Set(
                cacheKey,
                tree,
                new MemoryCacheEntryOptions()
                 .SetSlidingExpiration(TimeSpan.FromSeconds(_options.CacheDurationInSeconds))
                 );

            return Task.CompletedTask;
        }
    }
}
