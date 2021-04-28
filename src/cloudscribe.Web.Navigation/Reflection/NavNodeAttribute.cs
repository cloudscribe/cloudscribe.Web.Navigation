using System;
using System.Collections.Generic;
using System.Text;

namespace cloudscribe.Web.Navigation
{
    /// <summary>
    /// SiteMap node attribute, used to decorate action methods with SiteMap node metadata
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NavNodeAttribute : Attribute
    {
        /// <summary>required field for most nodes. Only the RootNode can be without a parent.</summary>
        public string ParentKey { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Page { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string NamedRoute { get; set; } = string.Empty;
        public bool ExcludeFromSearchSiteMap { get; set; } = false;
        public bool HideFromAuthenticated { get; set; } = false;
        public bool HideFromAnonymous { get; set; } = false;
        public string PreservedRouteParameters { get; set; } = string.Empty;
        public string ComponentVisibility { get; set; } = string.Empty;
        public string AuthorizationPolicy { get; set; } = string.Empty;
        public string ViewRoles { get; set; } = string.Empty;
        public string CustomData { get; set; } = string.Empty;
        public bool IsClickable { get; set; } = true;
        public string IconCssClass { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public string MenuDescription { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        /// <summary> The value must be a JSON string that represents a dictionary of key-value pairs. 
        /// Example: { "name1": "value1", "name2": "value2" }</summary>
        public string DataAttributesJson { get; set; } = string.Empty;
        public int Order { get; set; }
        public Type ResourceType { get; set; }

    }
}
