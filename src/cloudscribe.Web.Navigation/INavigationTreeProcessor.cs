using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation
{
    public interface INavigationTreeProcessor
    {
        Task ProcessTree(TreeNode<NavigationNode> rootNode);
    }
}
