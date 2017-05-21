using Microsoft.AspNetCore.Mvc;

namespace cloudscribe.Web.Navigation
{

    public interface IFindCurrentNode
    {
        TreeNode<NavigationNode> FindNode(
            TreeNode<NavigationNode> rootNode,
            IUrlHelper urlHelper,
            string currentUrl,
            string urlPrefix = "");
    }
}
