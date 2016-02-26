

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    public interface ITreeCache
    {
        Task<TreeNode<NavigationNode>> GetTree(string cacheKey);
        void AddToCache(TreeNode<NavigationNode> tree, string cacheKey);
        
    }
}
