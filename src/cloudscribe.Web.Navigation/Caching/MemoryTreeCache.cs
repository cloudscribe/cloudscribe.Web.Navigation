// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2016-02-26
// Last Modified:			2019-09-28
// 

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public class MemoryTreeCache : ITreeCache
    {
        public MemoryTreeCache(
            IMemoryCache cache,
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

        private readonly IMemoryCache _cache;
        private readonly TreeCacheOptions _options;
        private readonly ITreeCacheKeyResolver _cacheKeyResolver;
        private readonly IEnumerable<INavigationTreeBuilder> _treeBuilders;

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

        public Task ClearTreeCache(string cacheKey)
        {
            _cache.Remove(cacheKey);

            return Task.CompletedTask;
        }

        public async Task ClearTreeCache()
        {
            foreach (var builder in _treeBuilders)
            {
                var cacheKey = await _cacheKeyResolver.GetCacheKey(builder);
                _cache.Remove(cacheKey);
            }

            
        }
    }
}
