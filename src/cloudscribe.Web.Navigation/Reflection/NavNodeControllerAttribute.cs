using System;
using System.Collections.Generic;
using System.Text;

namespace cloudscribe.Web.Navigation
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NavNodeControllerAttribute : Attribute
    {
        /// <summary>you may set this field and use: 
        /// [NavNode(Key="{Prefix}NodeName", ParentKey="{Prefix}ParentName"] 
        /// then "{Prefix}" be replaced with this field automatically.
        /// </summary>
        public string KeyPrefix { get; set; } = string.Empty;
    }
}
