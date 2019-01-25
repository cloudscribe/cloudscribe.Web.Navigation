// Author:					Joe Audette
// Created:					2015-07-10
// Last Modified:			2015-07-15
// 


using Microsoft.AspNetCore.Mvc;
using System;

namespace cloudscribe.Web.Navigation
{
    public static class NavigationNodeExtensions
    {

        [Obsolete("This method is obsolete and never worked right, it will be removed in a future version. Please use the overload that takes an IUrlHelper")]
        public static string ResolveUrl(this NavigationNode node)
        {
            if (!string.IsNullOrWhiteSpace(node.Url)) return node.Url;
            string url = string.Empty;
            if((!string.IsNullOrWhiteSpace(node.Controller))&&(!string.IsNullOrWhiteSpace(node.Action)))
            {
                if(node.Action == "Index")
                {
                    url = "~/" + node.Controller;
                }
                else
                {
                    url = "~/" + node.Controller + "/" + node.Action;
                }
                
            }

            return url;
        }

        public static string ResolveUrl(this NavigationNode node, IUrlHelper urlHelper)
        {
            string urlToUse = string.Empty;

            if ((!string.IsNullOrWhiteSpace(node.Controller)) && (!string.IsNullOrWhiteSpace(node.Action)))
            {
                urlToUse = urlHelper.Action(node.Action, node.Controller, new { area = node.Area });    
            }
            else if (!string.IsNullOrWhiteSpace(node.NamedRoute))
            {
                urlToUse = urlHelper.RouteUrl(node.NamedRoute);
            }
                
            if (string.IsNullOrEmpty(urlToUse)) { return node.Url; }
           
            return urlToUse;
        }
    }
}
