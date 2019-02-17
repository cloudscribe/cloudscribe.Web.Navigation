using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public interface ITreeCache
    {
        Task<TreeNode<NavigationNode>> GetTree(string cacheKey);
        Task AddToCache(TreeNode<NavigationNode> tree, string cacheKey);
        Task ClearTreeCache(string cacheKey);


    }
}
