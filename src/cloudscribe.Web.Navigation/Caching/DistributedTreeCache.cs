// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-10-12
// Last Modified:			2019-02-17
// 

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public class DistributedTreeCache : ITreeCache
    {
        public DistributedTreeCache(
            IDistributedCache cache,
            ITreeCacheKeyResolver cacheKeyResolver,
            IOptions<TreeCacheOptions> optionsAccessor = null)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = optionsAccessor?.Value;
            if (_options == null) _options = new TreeCacheOptions(); //default
        }

        private readonly IDistributedCache _cache;
        private readonly TreeCacheOptions _options;
        private readonly ITreeCacheKeyResolver _cacheKeyResolver;

        public async Task<TreeNode<NavigationNode>> GetTree(string cacheKey)
        {

            var tree = await _cache.GetAsync<TreeNode<NavigationNode>>(cacheKey);
            return tree;

        }

        public async Task AddToCache(TreeNode<NavigationNode> tree, string cacheKey)
        {
            var options = new DistributedCacheEntryOptions();
            options.SetSlidingExpiration(TimeSpan.FromSeconds(_options.CacheDurationInSeconds));
            await _cache.SetAsync<TreeNode<NavigationNode>>(cacheKey, tree, options);

        }

        public async Task ClearTreeCache(string cacheKey)
        {
            await _cache.RemoveAsync(cacheKey);
        }

        



    }
}
