using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace cloudscribe.Web.Navigation
{
    /// <summary>
    /// </summary>
    public class NavigationTreeReflectionConverter
    {
        public async Task<TreeNode<NavigationNode>> ScanAndMerge(
            NavigationTreeBuilderService service,
            string includeAssembliesForScan,
            TreeNode<NavigationNode> treeRoot
            )
        {
           return await Task.Run<TreeNode<NavigationNode>>(() =>
           {
               var nodes = Scan(includeAssembliesForScan);
               treeRoot = Merge(nodes, treeRoot);
               return treeRoot;
           });
        }

        private class NavigationNodeWithParent
        {
            public NavigationNode Node { get; set; }
            public string ParentKey { get; set; }
        }

        private IList<NavigationNodeWithParent> Scan(string includeAssembliesForScan)
        {
            var list = new List<NavigationNodeWithParent>();
            // scan and collect NavNodeAttributes
            var assemblyNames = includeAssembliesForScan
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim());
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(m => assemblyNames.Contains(new AssemblyName(m.FullName).Name));

            foreach(var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(m => 
                    typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(m)
                    );
                foreach(var type in types)
                {
                    var controllerAttr = type.GetCustomAttribute(typeof(NavNodeControllerAttribute), false) 
                        as NavNodeControllerAttribute;
                    var areaAttr = type.GetCustomAttribute(typeof(AreaAttribute), true)
                        as AreaAttribute;
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.GetCustomAttributes(typeof(NavNodeAttribute), true).Any());
                    foreach(var method in methods)
                    {
                        var attribute = method.GetCustomAttribute(typeof(NavNodeAttribute), true)
                            as NavNodeAttribute;
                        if (attribute != null)
                        {
                            var node = ConvertMvc(type, method, attribute, controllerAttr, areaAttr);
                            list.Add(node);
                        }
                    }
                }

                //TODO razor pages
                //var types2 = assembly.GetTypes().Where(m =>
                //    typeof(Microsoft.AspNetCore.Mvc.RazorPages.PageModel).IsAssignableFrom(m)
                //    );
                //foreach (var type in types2)
                //{
                //    var attribute = type.GetCustomAttribute(typeof(NavNodeAttribute), true)
                //        as NavNodeAttribute;
                //    var areaAttr = type.GetCustomAttribute(typeof(AreaAttribute), false)
                //        as AreaAttribute;
                //    if (attribute != null)
                //    {
                //        var node = ConvertRazorPage(type, attribute, areaAttr);
                //        list.Add(node);
                //    }
                //}
            }
            return list;
        }

        private NavigationNodeWithParent ConvertMvc(Type type, MethodInfo method,
            NavNodeAttribute attribute, 
            NavNodeControllerAttribute controllerAttr, 
            AreaAttribute areaAttr)
        {
            var node = Convert(attribute);
            var prefix = "";
            if (controllerAttr != null) prefix = controllerAttr.KeyPrefix;
            node.Key = attribute.Key;
            if (string.IsNullOrEmpty(node.Key))
            {
                node.Key = Guid.NewGuid().ToString();
            }
            if (!string.IsNullOrEmpty(node.Key) && node.Key.StartsWith("{Prefix}"))
            {
                node.Key = prefix + node.Key.Substring("{Prefix}".Length);
            }
            var parentKey = attribute.ParentKey;
            if (!string.IsNullOrEmpty(parentKey) && parentKey.StartsWith("{Prefix}"))
            {
                parentKey = prefix + parentKey.Substring("{Prefix}".Length);
            }

            node.Area = string.Empty;
            if (areaAttr != null)
            {
                node.Area = areaAttr.RouteValue;
            }
            if (!string.IsNullOrEmpty(attribute.Area))
            {
                node.Area = attribute.Area;
            }

            if (!string.IsNullOrEmpty(attribute.Url))
            {
                node.Url = attribute.Url;
            }
            else
            {
                node.Controller = type.Name.Substring(0, type.Name.IndexOf("Controller"));

                node.Action = method.Name;
                var actioNameAttr = method.GetCustomAttributes(typeof(ActionNameAttribute), true)
                    .FirstOrDefault() as ActionNameAttribute;
                if (actioNameAttr != null)
                {
                    node.Action = actioNameAttr.Name;
                }
            }
            return new NavigationNodeWithParent() { Node = node, ParentKey = parentKey };
        }

        //private NavigationNodeWithParent ConvertRazorPage(Type type, 
        //    NavNodeAttribute attribute, 
        //    AreaAttribute areaAttr)
        //{
        //    var node = Convert(attribute);
        //    node.Key = attribute.Key;
        //    if (string.IsNullOrEmpty(node.Key))
        //    {
        //        node.Key = Guid.NewGuid().ToString();
        //    }
        //    var parentKey = attribute.ParentKey;

        //    node.Area = string.Empty;
        //    if (areaAttr != null)
        //    {
        //        node.Area = areaAttr.RouteValue;
        //    }
        //    if (!string.IsNullOrEmpty(attribute.Area))
        //    {
        //        node.Area = attribute.Area;
        //    }

        //    if (!string.IsNullOrEmpty(attribute.Url))
        //    {
        //        node.Url = attribute.Url;
        //    }
        //    else
        //    {
        //        node.Page = attribute.Page;
        //    }
        //    return new NavigationNodeWithParent() { Node = node, ParentKey = parentKey };
        //}


        private NavigationNode Convert(NavNodeAttribute attribute)
        {
            var node = new NavigationNode();
            if (attribute.ResourceType == null)
            {
                node.Text = attribute.Text ?? string.Empty;
                node.Title = attribute.Title ?? string.Empty;
                node.MenuDescription = attribute.MenuDescription ?? string.Empty;
            }
            else
            {
                node.Text = GetResourceString(attribute.ResourceType, attribute.Text);
                node.Title = GetResourceString(attribute.ResourceType, attribute.Title);
                node.MenuDescription = GetResourceString(attribute.ResourceType, attribute.MenuDescription);
            }
            node.NamedRoute = attribute.NamedRoute ?? string.Empty;
            node.ExcludeFromSearchSiteMap = attribute.ExcludeFromSearchSiteMap;
            node.HideFromAuthenticated = attribute.HideFromAuthenticated;
            node.HideFromAnonymous = attribute.HideFromAnonymous;
            node.PreservedRouteParameters = attribute.PreservedRouteParameters ?? string.Empty;
            node.ComponentVisibility = attribute.ComponentVisibility ?? string.Empty;
            node.AuthorizationPolicy = attribute.AuthorizationPolicy ?? string.Empty;
            node.ViewRoles = attribute.ViewRoles ?? string.Empty;
            node.CustomData = attribute.CustomData ?? string.Empty;
            node.IsClickable = attribute.IsClickable;
            node.IconCssClass = attribute.IconCssClass ?? string.Empty;
            node.CssClass = attribute.CssClass ?? string.Empty;
            node.Target = attribute.Target ?? string.Empty;
            node.Order = attribute.Order;
            if (!string.IsNullOrEmpty(attribute.DataAttributesJson))
            {
                var list = new List<DataAttribute>();
                try
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>
                        (attribute.DataAttributesJson);
                    foreach(var key in dict.Keys)
                    {
                        var value = dict[key];
                        if (value != null)
                        {
                            var da = new DataAttribute();
                            da.Attribute = key;
                            da.Value = dict[key].ToString();
                            list.Add(da);
                        }
                    }
                }
                catch
                {
                }
                node.DataAttributes = list;
            }

            return node;
        }
        private string GetResourceString(Type type, string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName)) return string.Empty;
            var prop = type.GetProperty(resourceName, BindingFlags.Static | BindingFlags.Public);
            if (prop == null) return resourceName;
            return prop.GetGetMethod().Invoke(null, null) as string;
        }


        private TreeNode<NavigationNode> Merge(IList<NavigationNodeWithParent> nodes, 
            TreeNode<NavigationNode> treeRoot)
        {
            var nodesAlreadyAdded = new HashSet<string>();
            #region process the RootNode
            if (treeRoot == null)
            {
                var homeNode = nodes.FirstOrDefault(m => string.IsNullOrEmpty(m.ParentKey)
                    || m.ParentKey == m.Node.Key);
                if (homeNode == null)
                {
                    var rootNav = new NavigationNode();
                    rootNav.Key = "RootNode";
                    //rootNav.IsRootNode = true;
                    rootNav.Text = "Missing RootNode";
                    rootNav.Url = "/";
                    treeRoot = new TreeNode<NavigationNode>(rootNav);
                }
                treeRoot = new TreeNode<NavigationNode>(homeNode.Node);
                nodesAlreadyAdded.Add(homeNode.Node.Key);
            }
            else
            {
                foreach(var node in treeRoot.Flatten())
                {
                    nodesAlreadyAdded.Add(node.Key);
                }
            }
            #endregion

            var sourceNodesByParent = nodes.ToLookup(n => n.ParentKey);
            var sourceNodes = new List<NavigationNodeWithParent>(nodes);
            var nodesAddedThisIteration = 0;
            do
            {
                nodesAddedThisIteration = 0;
                foreach (var nodeWithParent in sourceNodes.ToArray())
                {
                    if (nodesAlreadyAdded.Contains(nodeWithParent.Node.Key))
                    {
                        continue;
                    }

                    var parentNode = treeRoot.FindByKey(nodeWithParent.ParentKey);
                    if (parentNode != null)
                    {
                        var currentNode = parentNode.AddChild(nodeWithParent.Node);
                        nodesAlreadyAdded.Add(nodeWithParent.Node.Key);
                        sourceNodes.Remove(nodeWithParent);
                        nodesAddedThisIteration += 1;

                        // Add the rest of the tree branch below the current node
                        this.AddDescendantNodes(currentNode, sourceNodes, sourceNodesByParent, nodesAlreadyAdded);
                    }
                }
            }
            while (nodesAddedThisIteration > 0 && sourceNodes.Count > 0);

            return treeRoot;
        }

        private void AddDescendantNodes(
            TreeNode<NavigationNode> currentNode,
            List<NavigationNodeWithParent> sourceNodes,
            ILookup<string, NavigationNodeWithParent> sourceNodesByParent,
            HashSet<string> nodesAlreadyAdded)
        {
            if (sourceNodes.Count == 0)
            {
                return;
            }

            var children = sourceNodesByParent[currentNode.Value.Key];
            if (children.Count() == 0)
            {
                return;
            }

            foreach (var child in children)
            {
                if (sourceNodes.Count == 0)
                {
                    return;
                }

                var childNode = currentNode.AddChild(child.Node);
                nodesAlreadyAdded.Add(child.Node.Key);
                sourceNodes.Remove(child);

                if (sourceNodes.Count == 0)
                {
                    return;
                }

                this.AddDescendantNodes(childNode, sourceNodes, sourceNodesByParent, nodesAlreadyAdded);
            }
        }

    }
}
