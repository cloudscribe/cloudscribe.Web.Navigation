// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-10-12
// Last Modified:			2016-02-26
// 

using cloudscribe.Web.Navigation.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation
{
    public class NavigationTreeBuilderService
    {
        public NavigationTreeBuilderService(
            IEnumerable<INavigationTreeBuilder> treeBuilders,
            IOptions<NavigationOptions> navigationOptionsAccessor,
            ITreeCache treeCache = null
            )
        {
            if (treeBuilders == null) { throw new ArgumentNullException(nameof(treeBuilders)); }
            if (navigationOptionsAccessor == null) { throw new ArgumentNullException(nameof(navigationOptionsAccessor)); }

            this.treeCache = treeCache ?? new NotCachedTreeCache();
            builders = treeBuilders;
            navOptions = navigationOptionsAccessor.Value;

        }

        private ITreeCache treeCache;
        private NavigationOptions navOptions;

        private IEnumerable<INavigationTreeBuilder> builders;

        public INavigationTreeBuilder GetRootTreeBuilder()
        {
            return GetTreeBuilder(navOptions.RootTreeBuilderName);
        }

        public INavigationTreeBuilder GetTreeBuilder(string name)
        {
            foreach(var t in builders)
            {
                if(t.Name == name) { return t; }
            }

            return null;
        }

        public async Task<TreeNode<NavigationNode>> GetTree()
        {
            //TODO: do we need a more specific cache key? ie could multiple trees have been created by the same buildername

            var builder = GetRootTreeBuilder();
            var cacheKey = builder.Name;
            var tree = await treeCache.GetTree(cacheKey).ConfigureAwait(false);
            if(tree != null) { return tree; }
            tree = await builder.BuildTree(this).ConfigureAwait(false);
            treeCache.AddToCache(tree, cacheKey);

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


    }
}
