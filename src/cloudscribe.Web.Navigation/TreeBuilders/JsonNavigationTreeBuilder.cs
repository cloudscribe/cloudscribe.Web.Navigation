// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-14
// Last Modified:			2017-07-31
// 

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation
{
    public class JsonNavigationTreeBuilder : INavigationTreeBuilder
    {
        public JsonNavigationTreeBuilder(
            IWebHostEnvironment appEnv,
            IOptions<NavigationOptions> navigationOptionsAccessor,
            IEnumerable<INavigationTreeProcessor> treeProcessors,
            ILogger<JsonNavigationTreeBuilder> logger
            )
        {
            if (navigationOptionsAccessor == null) { throw new ArgumentNullException(nameof(navigationOptionsAccessor)); }

            _env = appEnv ?? throw new ArgumentNullException(nameof(appEnv));
            _navOptions = navigationOptionsAccessor.Value;
            _treeProcessors = treeProcessors;
            _log = logger;
           
        }

       
        
       private readonly IWebHostEnvironment _env;
        private readonly NavigationOptions _navOptions;
        private readonly IEnumerable<INavigationTreeProcessor> _treeProcessors;
        private readonly ILogger _log;
        private TreeNode<NavigationNode> rootNode = null;

        public string Name
        { 
            get { return "cloudscribe.Web.Navigation.JsonNavigationTreeBuilder"; }
        }

        public async Task<TreeNode<NavigationNode>> BuildTree(NavigationTreeBuilderService service)
        {
            
            if (rootNode == null)
            { 
                rootNode = await BuildTree();

                foreach (var processor in _treeProcessors)
                {
                    await processor.ProcessTree(rootNode);
                }
              
            }

            return rootNode;
        }

        private async Task<TreeNode<NavigationNode>> BuildTree()
        {
            string filePath = ResolveFilePath();

            if(!File.Exists(filePath))
            {
                _log.LogError("unable to build navigation tree, could not find the file " + filePath);

                return null;
            }

            string json;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    json = await streamReader.ReadToEndAsync();
                }
            }

            TreeNode<NavigationNode> result = BuildTreeFromJson(json);

            return result;
            
        }

        private string ResolveFilePath()
        {
            string filePath = _env.ContentRootPath + Path.DirectorySeparatorChar
                + _navOptions.NavigationMapJsonFileName;

            return filePath;
        }


        // started to make this async since there are async methods of deserializeobject
        // but found this thread which says not to use them as there is no benefit
        //https://github.com/JamesNK/Newtonsoft.Json/issues/66
        public TreeNode<NavigationNode> BuildTreeFromJson(string jsonString)
        {
            TreeNode<NavigationNode> rootNode =
                    JsonConvert.DeserializeObject<TreeNode<NavigationNode>>(jsonString, new NavigationTreeJsonConverter());


            return rootNode;

        }
    }
}
