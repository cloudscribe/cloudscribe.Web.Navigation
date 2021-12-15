// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-14
// Last Modified:			2019-02-14
// 

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace cloudscribe.Web.Navigation
{
    public class XmlNavigationTreeBuilder : INavigationTreeBuilder
    {
        public XmlNavigationTreeBuilder(
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
            get { return Constants.XmlNavigationTreeBuilderName; }
        }

        public async Task<TreeNode<NavigationNode>> BuildTree(
            NavigationTreeBuilderService service)
        {
           
            if (rootNode == null)
            { 
                rootNode = await BuildTreeInternal(service);  
                foreach(var processor in _treeProcessors)
                {
                    await processor.ProcessTree(rootNode);
                }
            }

            return rootNode;
        }

        private string ResolveFilePath()
        {
            string filePath = _env.ContentRootPath + Path.DirectorySeparatorChar
                + _navOptions.NavigationMapXmlFileName;

            return filePath;
        }

        private async Task<TreeNode<NavigationNode>> BuildTreeInternal(NavigationTreeBuilderService service)
        {
            string filePath = ResolveFilePath();

            if (!File.Exists(filePath))
            {
                _log.LogError("unable to build navigation tree, could not find the file " + filePath);

                NavigationNode rootNav = new NavigationNode();
                rootNav.Key = "filenotfound";
                //rootNav.IsRootNode = true;
                rootNav.Text = filePath + " not found";
                rootNav.Url = "/";
                var treeRoot = new TreeNode<NavigationNode>(rootNav);

                return treeRoot;
            }

            string xml;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    xml = await streamReader.ReadToEndAsync();
                }
            }

            XDocument doc = XDocument.Parse(xml);

            NavigationTreeXmlConverter converter = new NavigationTreeXmlConverter();

            TreeNode<NavigationNode> result = await converter.FromXml(doc, service).ConfigureAwait(false);

            return result;

        }

    }
}
