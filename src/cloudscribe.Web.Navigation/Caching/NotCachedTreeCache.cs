using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public class NotCachedTreeCache : ITreeCache
    {
        public Task<TreeNode<NavigationNode>> GetTree(string cacheKey)
        {
            TreeNode<NavigationNode> tree = null;
            
            return Task.FromResult(tree);
        }

        public Task AddToCache(TreeNode<NavigationNode> tree, string cacheKey)
        {
            //do nothing
            return Task.CompletedTask;
        }

        
    }
}
