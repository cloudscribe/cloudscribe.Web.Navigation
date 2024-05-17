using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public interface IDOMTreeCache
    {
        Task ClearDOMTreeCache(string cacheKey);
        Task<string> GetDOMTree(string cacheKey);
        Task StoreDOMTree(string cacheKey, string tree, int expirationSeconds);
    }
}