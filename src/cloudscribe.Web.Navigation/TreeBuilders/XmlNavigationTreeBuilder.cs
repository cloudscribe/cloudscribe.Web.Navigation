// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-14
// Last Modified:			2016-05-17
// 

using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;

namespace cloudscribe.Web.Navigation
{
    public class XmlNavigationTreeBuilder : INavigationTreeBuilder
    {
        public XmlNavigationTreeBuilder(
            IHostingEnvironment appEnv,
            IOptions<NavigationOptions> navigationOptionsAccessor,
            ILogger<XmlNavigationTreeBuilder> logger)
        {
            if (appEnv == null) { throw new ArgumentNullException(nameof(appEnv)); }
            if (logger == null) { throw new ArgumentNullException(nameof(logger)); }
            if (navigationOptionsAccessor == null) { throw new ArgumentNullException(nameof(navigationOptionsAccessor)); }

            this.appEnv = appEnv;
            navOptions = navigationOptionsAccessor.Value;
            log = logger;
            
        }

        private IHostingEnvironment appEnv;
        private NavigationOptions navOptions;
        private ILogger log;
        private TreeNode<NavigationNode> rootNode = null;

        public string Name
        {
            get { return "cloudscribe.Web.Navigation.XmlNavigationTreeBuilder"; }
        }

        public async Task<TreeNode<NavigationNode>> BuildTree(
            NavigationTreeBuilderService service)
        {
           
            if (rootNode == null)
            { 
                rootNode = await BuildTreeInternal(service);  
            }

            return rootNode;
        }

        private string ResolveFilePath()
        {
            string filePath = appEnv.ContentRootPath + Path.DirectorySeparatorChar
                + navOptions.NavigationMapXmlFileName;

            return filePath;
        }

        private async Task<TreeNode<NavigationNode>> BuildTreeInternal(NavigationTreeBuilderService service)
        {
            string filePath = ResolveFilePath();

            if (!File.Exists(filePath))
            {
                log.LogError("unable to build navigation tree, could not find the file " + filePath);

                NavigationNode rootNav = new NavigationNode();
                rootNav.Key = "filenotfound";
                rootNav.IsRootNode = true;
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
