using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation
{
    /// <summary>
    /// </summary>
    public class ReflectionNavigationTreeBuilder : INavigationTreeBuilder
    {
        public ReflectionNavigationTreeBuilder(
            IWebHostEnvironment appEnv,
            IOptions<NavigationOptions> navigationOptionsAccessor,
            IEnumerable<INavigationTreeProcessor> treeProcessors,
            ILogger<XmlNavigationTreeBuilder> logger)
        {
            if (appEnv == null) { throw new ArgumentNullException(nameof(appEnv)); }
            if (logger == null) { throw new ArgumentNullException(nameof(logger)); }
            if (navigationOptionsAccessor == null) { throw new ArgumentNullException(nameof(navigationOptionsAccessor)); }

            _env = appEnv;
            _navOptions = navigationOptionsAccessor.Value;
            _treeProcessors = treeProcessors;
            _log = logger;

        }

        private readonly IWebHostEnvironment _env;
        private readonly NavigationOptions _navOptions;
        private readonly ILogger _log;
        private TreeNode<NavigationNode> rootNode = null;
        private readonly IEnumerable<INavigationTreeProcessor> _treeProcessors;

        public string Name
        {
            get { return Constants.ReflectionNavigationTreeBuilderName; }
        }

        public async Task<TreeNode<NavigationNode>> BuildTree(NavigationTreeBuilderService service)
        {
            if (rootNode == null)
            {
                rootNode = await BuildTreeInternal(service);
                foreach (var processor in _treeProcessors)
                {
                    await processor.ProcessTree(rootNode);
                }
            }

            return rootNode;
        }

        private async Task<TreeNode<NavigationNode>> BuildTreeInternal(NavigationTreeBuilderService service)
        {

            if (string.IsNullOrEmpty(_navOptions.IncludeAssembliesForScan))
            {
                _log.LogError("unable to build navigation tree, 'IncludeAssembliesForScan' not specified. ");

                var rootNav = new NavigationNode();
                rootNav.Key = "RootNode";
                //rootNav.IsRootNode = true;
                rootNav.Text = "Missing config for IncludeAssembliesForScan";
                rootNav.Url = "/";
                var treeRoot = new TreeNode<NavigationNode>(rootNav);

                return treeRoot;
            }

            var helper = new NavigationTreeReflectionConverter();
            return await helper.ScanAndMerge(service, _navOptions.IncludeAssembliesForScan, null).ConfigureAwait(false);
        }
    }
}
