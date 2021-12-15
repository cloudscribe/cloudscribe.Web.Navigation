// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-10-12
// Last Modified:			2019-09-28
// 

using cloudscribe.Web.Navigation.Caching;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation
{
    public class NavigationTreeBuilderService
    {
        public NavigationTreeBuilderService(
            IEnumerable<INavigationTreeBuilder> treeBuilders,
            ITreeCacheKeyResolver cacheKeyResolver,
            IOptions<NavigationOptions> navigationOptionsAccessor,
            ITreeCache treeCache = null
            )
        {
            if (navigationOptionsAccessor == null) { throw new ArgumentNullException(nameof(navigationOptionsAccessor)); }

            _cacheKeyResolver = cacheKeyResolver;
            _treeCache = treeCache ?? new NotCachedTreeCache();
            _builders = treeBuilders ?? throw new ArgumentNullException(nameof(treeBuilders));
            _navOptions = navigationOptionsAccessor.Value;

        }

        private readonly ITreeCache _treeCache;
        private readonly ITreeCacheKeyResolver _cacheKeyResolver;
        private readonly NavigationOptions _navOptions;
        private readonly IEnumerable<INavigationTreeBuilder> _builders;

        public INavigationTreeBuilder GetRootTreeBuilder()
        {
            return GetTreeBuilder(_navOptions.RootTreeBuilderName);
        }

        public INavigationTreeBuilder GetTreeBuilder(string name)
        {
            foreach(var t in _builders)
            {
                if(t.Name == name) { return t; }
            }

            return null;
        }

        public async Task<TreeNode<NavigationNode>> GetTree()
        {
            var builder = GetRootTreeBuilder();
            var cacheKey = await _cacheKeyResolver.GetCacheKey(builder);
            var tree = await _treeCache.GetTree(cacheKey).ConfigureAwait(false);
            if(tree != null) { return tree; }
            tree = await builder.BuildTree(this).ConfigureAwait(false);
            if (_navOptions.RootTreeBuilderName != Constants.ReflectionNavigationTreeBuilderName &&
                !string.IsNullOrEmpty(_navOptions.IncludeAssembliesForScan))
            {
                var helper = new NavigationTreeReflectionConverter();
                await helper.ScanAndMerge(this, _navOptions.IncludeAssembliesForScan, tree).ConfigureAwait(false);
            }
            if (_navOptions.EnableSorting)
            {
                SortTreeNode(tree);
            }
            await _treeCache.AddToCache(tree, cacheKey);

            return tree;
        }

        public async Task<TreeNode<NavigationNode>> GetTree(string builderName)
        {
            //this one is only called if using nested builders so
            // we should not cache here, the result will be cached in the main tree
            // after all the builders have built it up

            //var cacheKey = builderName;
            //var tree = await treeCache.GetTree(cacheKey).ConfigureAwait(false);
            //if (tree != null) { return tree; }
            var builder = GetTreeBuilder(builderName);
            var tree = await builder.BuildTree(this).ConfigureAwait(false);

            return tree;
        }

        public async Task ClearTreeCache()
        {
            foreach (var builder in _builders)
            {
                var cacheKey = await _cacheKeyResolver.GetCacheKey(builder);
                await _treeCache.ClearTreeCache(cacheKey);
            }

        }

        private void SortTreeNode(TreeNode<NavigationNode> treeNode)
        {
            if (treeNode.Children.Count > 0)
            {
                treeNode.Sort();
                foreach (var child in treeNode.Children)
                {
                    SortTreeNode(child);
                }
            }
        }

    }
}
