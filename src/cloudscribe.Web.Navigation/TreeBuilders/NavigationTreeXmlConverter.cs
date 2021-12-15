﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-14
// Last Modified:			2016-11-16
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace cloudscribe.Web.Navigation
{
    public class NavigationTreeXmlConverter
    {
        public string ToXmlString(TreeNode<NavigationNode> node)
        {
            StringBuilder s = new StringBuilder();
            Encoding encoding = Encoding.Unicode;
            using (StringWriter stringWriter = new StringWriter(s))
            {

                using (XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartDocument();
                    WriteNode(node, writer);
                }
            }

            return s.ToString();
        }

        private void WriteNode(TreeNode<NavigationNode> node, XmlWriter writer)
        {
            writer.WriteStartElement("NavNode");

            if (!string.IsNullOrEmpty(node.Value.Key))
            {
                writer.WriteAttributeString("key", node.Value.Key);
            }

            //if (node.Value.ParentKey.Length > 0)
            //{
            //    writer.WriteAttributeString("parentKey", node.Value.ParentKey);
            //}

            if (!string.IsNullOrEmpty(node.Value.Controller))
            {
                writer.WriteAttributeString("controller", node.Value.Controller);
            }

            if (!string.IsNullOrEmpty(node.Value.Action))
            {
                writer.WriteAttributeString("action", node.Value.Action);
            }

            if (!string.IsNullOrEmpty(node.Value.Area))
            {
                writer.WriteAttributeString("area", node.Value.Area);
            }

            if (!string.IsNullOrEmpty(node.Value.Page))
            {
                writer.WriteAttributeString("page", node.Value.Page);
            }

            if (String.IsNullOrEmpty(node.Value.NamedRoute))
            {
                writer.WriteAttributeString("namedRoute", node.Value.NamedRoute);
            }

            if (!string.IsNullOrEmpty(node.Value.Text))
            {
                writer.WriteAttributeString("text", node.Value.Text);
            }

            if (!string.IsNullOrEmpty(node.Value.Url))
            {
                writer.WriteAttributeString("url", node.Value.Url);
            }

            if (!string.IsNullOrEmpty(node.Value.PreservedRouteParameters))
            {
                writer.WriteAttributeString("preservedRouteParameters", node.Value.PreservedRouteParameters);
            }

            if (!string.IsNullOrEmpty(node.Value.ComponentVisibility))
            {
                writer.WriteAttributeString("componentVisibility", node.Value.ComponentVisibility);
            }

            if (!string.IsNullOrEmpty(node.Value.AuthorizationPolicy))
            {
                writer.WriteAttributeString("authorizationPolicy", node.Value.AuthorizationPolicy);
            }

            if (!string.IsNullOrEmpty(node.Value.ViewRoles))
            {
                writer.WriteAttributeString("viewRoles", node.Value.ViewRoles);
            }

            if (!string.IsNullOrEmpty(node.Value.CustomData))
            {
                writer.WriteAttributeString("customData", node.Value.CustomData);
            }

            //if (node.Value.IsRootNode)
            //{
            //    writer.WriteAttributeString("isRootNode", "true");
            //}

            if (node.Value.ExcludeFromSearchSiteMap)
            {
                writer.WriteAttributeString("excludeFromSearchSiteMap", "true");
            }

            //if (node.Value.ResourceName.Length > 0)
            //{
            //    writer.WriteAttributeString("resourceName", node.Value.ResourceName);
            //}

            //if (node.Value.ResourceTextKey.Length > 0)
            //{
            //    writer.WriteAttributeString("resourceTextKey", node.Value.ResourceTextKey);
            //}

            //if (node.Value.ResourceTitleKey.Length > 0)
            //{
            //    writer.WriteAttributeString("resourceTitleKey", node.Value.ResourceTitleKey);
            //}

            //if (!node.Value.IncludeAmbientValuesInUrl)
            //{
            //    writer.WriteAttributeString("includeAmbientValuesInUrl", "false");
            //}


            // children
            writer.WriteStartElement("Children");

            WriteChildNodes(node, writer);

            writer.WriteEndElement(); //Children

            if(node.Value.DataAttributes.Count > 0)
            {
                writer.WriteStartElement("DataAttributes");

                foreach(var att in node.Value.DataAttributes)
                {
                    writer.WriteStartElement("DataAttribute");

                    writer.WriteAttributeString("attribute", att.Attribute);
                    writer.WriteAttributeString("value", att.Value);

                    writer.WriteEndElement(); //DataAttribute
                }

                writer.WriteEndElement(); //DataAttributes
            }


            writer.WriteEndElement(); //NavNode
        }

        private void WriteChildNodes(TreeNode<NavigationNode> node, XmlWriter writer)
        {
            foreach (TreeNode<NavigationNode> child in node.Children)
            {
                WriteNode(child, writer);
            }
        }

        public async Task<TreeNode<NavigationNode>> FromXml(
            XDocument xml,
            NavigationTreeBuilderService service
            )
        { 
            if(xml.Root.Name != "NavNode") { throw new ArgumentException("Expected NavNode"); }

            TreeNode<NavigationNode> treeRoot;
            var builderName = GetNodeBuilderName(xml.Root);
            if (string.IsNullOrEmpty(builderName))
            {
                NavigationNode rootNav = BuildNavNode(xml.Root, service);
                treeRoot = new TreeNode<NavigationNode>(rootNav);
               
            }
            else
            {
                var otherBuilderRoot = await service.GetTree(builderName).ConfigureAwait(false);
                if(otherBuilderRoot.Value.ChildContainerOnly)
                {
                    NavigationNode rootNav = BuildNavNode(xml.Root, service);
                    treeRoot = new TreeNode<NavigationNode>(rootNav);
                    foreach(var firstChild in otherBuilderRoot.Children)
                    {
                        treeRoot.AddChild(firstChild);
                    }

                }
                else
                {
                    treeRoot = otherBuilderRoot;
                }
            }

            var childrenNode = xml.Root.Elements(XName.Get("Children"));
            if (childrenNode != null)
            {
                foreach (XElement childNode in childrenNode.Elements(XName.Get("NavNode")))
                {
                    var childBuilder = GetNodeBuilderName(childNode);
                    if (string.IsNullOrEmpty(childBuilder))
                    {
                        await AddChildNode(treeRoot, childNode, service).ConfigureAwait(false);
                    }
                    else
                    {
                        var appendToBuilderNode = AppendToBuilderNode(childNode);
                        var childTreeRoot = await service.GetTree(childBuilder).ConfigureAwait(false);
                        if(appendToBuilderNode)
                        {
                            var builderNode = BuildNavNode(childNode, service);
                            var bt = treeRoot.AddChild(builderNode);
                            foreach (var subChild in childTreeRoot.Children)
                            {
                                bt.AddChild(subChild);
                                
                            }
                        }
                        else
                        {
                            if (childTreeRoot.Value.ChildContainerOnly)
                            {
                                foreach (var subChild in childTreeRoot.Children)
                                {
                                    treeRoot.AddChild(subChild);
                                }
                            }
                            else
                            {
                                treeRoot.AddChild(childTreeRoot);
                            }
                        }
                        

                    }
                }
            }



            //foreach (XElement childrenNode in xml.Root.Elements(XName.Get("Children")))
            //{
            //    foreach (XElement childNode in childrenNode.Elements(XName.Get("NavNode")))
            //    {
            //        var childBuilder = GetNodeBuilderName(childNode);
            //        if(string.IsNullOrEmpty(childBuilder))
            //        {
            //            await AddChildNode(treeRoot, childNode, service).ConfigureAwait(false);
            //        }
            //        else
            //        {
            //            var child = await service.GetTree(childBuilder).ConfigureAwait(false);
            //            if(child.Value.ChildContainerOnly)
            //            {
            //                foreach(var subChild in child.Children)
            //                {
            //                    treeRoot.AddChild(subChild);
            //                }
            //            }
            //            else
            //            {
            //                treeRoot.AddChild(child);
            //            }

            //        }


            //    }

            //}

            return treeRoot;
        }

        private async Task AddChildNode(
            TreeNode<NavigationNode> node, 
            XElement xmlNode,
            NavigationTreeBuilderService service
            )
        {
            NavigationNode navNode = BuildNavNode(xmlNode, service);
            TreeNode<NavigationNode> navNodeT = node.AddChild(navNode);

            foreach (XElement childrenNode in xmlNode.Elements(XName.Get("Children")))
            {
                foreach (XElement childNode in childrenNode.Elements(XName.Get("NavNode")))
                {
                    var childBuilder = GetNodeBuilderName(childNode);
                    if (string.IsNullOrEmpty(childBuilder))
                    {
                        await AddChildNode(navNodeT, childNode, service).ConfigureAwait(false); //recursion
                    }
                    else
                    {
                        var appendToBuilderNode = AppendToBuilderNode(childNode);
                        var childTreeRoot = await service.GetTree(childBuilder).ConfigureAwait(false);
                        //if(appendToBuilderNode)
                        //{

                        //}
                        //else
                        //{
                            if (childTreeRoot.Value.ChildContainerOnly)
                            {
                                foreach (var subChild in childTreeRoot.Children)
                                {
                                    navNodeT.AddChild(subChild);
                                }
                            }
                            else
                            {
                                navNodeT.AddChild(childTreeRoot);
                            }
                        //}
                          
                    }

                       
                }
            }

            

        }

        private string GetNodeBuilderName(XElement xmlNode)
        {
            var tb = xmlNode.Attribute("treeBuilderName");
            if (tb != null) { return tb.Value; }

            return string.Empty;
        }

        private bool AppendToBuilderNode(XElement xmlNode)
        {
            var a = xmlNode.Attribute("treeBuilderAppendToBuilderNode");
            if (a != null) { return Convert.ToBoolean(a.Value); }

            return false;
        }

        private NavigationNode BuildNavNode(
            XElement xmlNode,
            NavigationTreeBuilderService service
            )
        {
            NavigationNode navNode = new NavigationNode();

            //var tb = xmlNode.Attribute("TreeBuilderName");
            //if (tb != null)
            //{
            //   return await service.GetTree(tb.Value).ConfigureAwait(false)
            //}

            var a = xmlNode.Attribute("key");
            if(a != null) {  navNode.Key = a.Value; }
            // automatically generate a key if
            if (string.IsNullOrEmpty(navNode.Key))
            {
                navNode.Key = Guid.NewGuid().ToString();
            }

            //a = xmlNode.Attribute("parentKey");
            //if (a != null) { navNode.ParentKey = a.Value; }

            a = xmlNode.Attribute("controller");
            if (a != null) { navNode.Controller = a.Value; }

            a = xmlNode.Attribute("action");
            if (a != null) { navNode.Action = a.Value; }

            a = xmlNode.Attribute("area");
            if (a != null) { navNode.Area = a.Value; }

            a = xmlNode.Attribute("page");
            if (a != null) { navNode.Page = a.Value; }

            a = xmlNode.Attribute("namedRoute");
            if(a == null) a = xmlNode.Attribute("named-route"); //this is not consistent was a mistake
            if (a != null) { navNode.NamedRoute = a.Value; }

            a = xmlNode.Attribute("text");
            if (a != null) { navNode.Text = ProcessResourceString(a.Value); }

            a = xmlNode.Attribute("title");
            if (a  != null) { navNode.Title = ProcessResourceString(a.Value); }

            a = xmlNode.Attribute("url");
            if (a != null) { navNode.Url = a.Value; }
            //else
            //{
            //    navNode.Url = navNode.ResolveUrl(); // this smells bad
            //}

            //a = xmlNode.Attribute("isRootNode");
            //if (a != null) { navNode.IsRootNode = Convert.ToBoolean(a.Value); }

            a = xmlNode.Attribute("excludeFromSearchSiteMap");
            if (a != null) { navNode.ExcludeFromSearchSiteMap = Convert.ToBoolean(a.Value); }

            

            a = xmlNode.Attribute("hideFromAuthenticated");
            if (a != null) { navNode.HideFromAuthenticated = Convert.ToBoolean(a.Value); }

            a = xmlNode.Attribute("hideFromAnonymous");
            if (a != null) { navNode.HideFromAnonymous = Convert.ToBoolean(a.Value); }

            //a = xmlNode.Attribute("includeAmbientValuesInUrl");
            //if (a != null) { navNode.IncludeAmbientValuesInUrl = Convert.ToBoolean(a.Value); }

            //a = xmlNode.Attribute("resourceName");
            //if (a != null) { navNode.ResourceName = a.Value; }

            //a = xmlNode.Attribute("resourceTextKey");
            //if (a != null) { navNode.ResourceTextKey = a.Value; }

            //a = xmlNode.Attribute("resourceTitleKey");
            //if (a != null) { navNode.ResourceTitleKey = a.Value; }

            a = xmlNode.Attribute("preservedRouteParameters");
            if (a != null) { navNode.PreservedRouteParameters = a.Value; }

            a = xmlNode.Attribute("componentVisibility");
            if (a != null) { navNode.ComponentVisibility = a.Value; }

            a = xmlNode.Attribute("authorizationPolicy");
            if (a != null) { navNode.AuthorizationPolicy = a.Value; }

            a = xmlNode.Attribute("viewRoles");
            if (a != null) { navNode.ViewRoles = a.Value; }

            a = xmlNode.Attribute("customData");
            if (a != null) { navNode.CustomData = a.Value; }


            a = xmlNode.Attribute("isClickable");
            if (a != null) { navNode.IsClickable = Convert.ToBoolean(a.Value); }

            a = xmlNode.Attribute("iconCssClass");
            if (a != null) { navNode.IconCssClass = a.Value; }

            a = xmlNode.Attribute("cssClass");
            if (a != null) { navNode.CssClass = a.Value; }

            a = xmlNode.Attribute("menuDescription");
            if (a != null) { navNode.MenuDescription = ProcessResourceString(a.Value); }

            a = xmlNode.Attribute("target");
            if (a != null) { navNode.Target = a.Value; }

            a = xmlNode.Attribute("order");
            if (a != null) { navNode.Order = Convert.ToInt32(a.Value); }

            var da = xmlNode.Element(XName.Get("DataAttributes"));
            if (da != null)
            {
                foreach (XElement childNode in da.Elements(XName.Get("DataAttribute")))
                {
                    var key = childNode.Attribute("attribute");
                    var val = childNode.Attribute("value");
                    if ((key != null) && (val != null))
                    {
                        var att = new DataAttribute();
                        att.Attribute = key.Value;
                        att.Value = val.Value;
                        navNode.DataAttributes.Add(att);
                    }


                }

            }


            return navNode;
        }

        /// <summary>
        /// support for the old resource string in ASP.NET Site Navigation, see:
        /// https://docs.microsoft.com/en-us/previous-versions/aspnet/ms178427(v=vs.100)?redirectedfrom=MSDN
        /// 
        /// Only use this for compatiblity with MvcSiteMapProvider.
        /// 
        /// format: $resources:ClassName,ResourceName,DefaultString (DefaultString is optional)
        /// </summary>
        private string ProcessResourceString(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > 11)
            {
                var tmp = value.Trim();
                if (tmp.ToLowerInvariant().StartsWith("$resources:"))
                {
                    tmp = tmp.Substring(11);
                    var pieces = tmp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pieces.Length >= 2)
                    {
                        var className = pieces[0].Trim();
                        var resourceName = pieces[1].Trim();
                        var defaultString = (pieces.Length >= 3 ? pieces[2] : resourceName);
                        var type = GetTypeFromAppDomain(className);
                        if (type == null) return defaultString;
                        var prop = type.GetProperty(resourceName, 
                            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        if (prop == null) return defaultString;
                        return prop.GetGetMethod().Invoke(null, null) as string;
                    }
                }
            }
            return value;
        }
        private Dictionary<string, Type> classesDict;
        private Type GetTypeFromAppDomain(string className)
        {
            if (classesDict == null) classesDict = new Dictionary<string, Type>();
            Type type = null;
            if (classesDict.ContainsKey(className))
            {
                type = classesDict[className];
            }
            else
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(className);
                    if (type != null)
                    {
                        classesDict[className] = type;
                        return type;
                    }
                }
                classesDict[className] = null; //not found
            }
            return type;
        }
    }

}
