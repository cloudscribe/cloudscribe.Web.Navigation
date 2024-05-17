using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public class DOMTreeCache : IDOMTreeCache
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedTreeCache> _logger;

        public DOMTreeCache(
            IDistributedCache cache,
            ILogger<DistributedTreeCache> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<string> GetDOMTree(string cacheKey)
        {
            var dom = await _cache.GetAsync<string>(cacheKey);
            return dom;
        }

        public async Task StoreDOMTree(string cacheKey, string tree, int expirationSeconds)
        {
            try
            {
                var options = new DistributedCacheEntryOptions();
                options.SetSlidingExpiration(TimeSpan.FromSeconds(expirationSeconds));
                await _cache.SetAsync<string>(cacheKey, tree, options);
                _logger.LogDebug($"Added navigation DOM tree to distributed cache: {cacheKey}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add navigation DOM tree to distributed cache: {cacheKey}");
            }
        }

        public async Task ClearDOMTreeCache(string cacheKey)
        {
            await _cache.RemoveAsync(cacheKey);
            // ((MemoryCache)_cache).Compact(1);
        }
    }
}
