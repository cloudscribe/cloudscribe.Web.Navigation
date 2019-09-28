// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-10-12
// Last Modified:			2019-09-29
// 

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public class DistributedTreeCache : ITreeCache
    {
        public DistributedTreeCache(
            IDistributedCache cache,
            ITreeCacheKeyResolver cacheKeyResolver,
            IEnumerable<INavigationTreeBuilder> treeBuilders,
            IOptions<TreeCacheOptions> optionsAccessor = null)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = optionsAccessor?.Value;
            if (_options == null) _options = new TreeCacheOptions(); //default
            _cacheKeyResolver = cacheKeyResolver;
            _treeBuilders = treeBuilders;
        }

        private readonly IDistributedCache _cache;
        private readonly TreeCacheOptions _options;
        private readonly ITreeCacheKeyResolver _cacheKeyResolver;
        private readonly IEnumerable<INavigationTreeBuilder> _treeBuilders;

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

        public async Task ClearTreeCache()
        {
            foreach (var builder in _treeBuilders)
            {
                var cacheKey = await _cacheKeyResolver.GetCacheKey(builder);
                await _cache.RemoveAsync(cacheKey);
            }

        }



    }
}
