using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public interface ITreeCacheKeyResolver
    {
        Task<string> GetCacheKey(INavigationTreeBuilder builder);
    }
}
