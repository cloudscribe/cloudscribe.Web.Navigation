using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public class DefaultCacheKeyResolver : ITreeCacheKeyResolver
    {
        public Task<string> GetCacheKey(INavigationTreeBuilder builder)
        {
            return Task.FromResult(builder.Name);
        }
    }
}
