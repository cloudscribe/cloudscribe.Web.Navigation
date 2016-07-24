// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2016-07-24
// Last Modified:			2016-07-24
// 


namespace cloudscribe.Web.Navigation
{
    public static class NavigationViewModelExtensions
    {
        public static string GetClass(this NavigationViewModel model, NavigationNode node, string inputClass = null, string activeClass = "active")
        {
            if (node == null) return null;
            if (model.CurrentNode != null && (node.EqualsNode(model.CurrentNode.Value)))
            {
                if (!string.IsNullOrEmpty(inputClass))
                {
                    inputClass = activeClass + " " + inputClass;
                }
                else
                {
                    inputClass = activeClass;
                }
            }
            if (string.IsNullOrEmpty(node.CssClass))
            {
                return inputClass;
            }
            else
            {
                if (!string.IsNullOrEmpty(inputClass))
                {
                    return inputClass + " " + node.CssClass;
                }
                return node.CssClass;
            }
        }

        public static string GetIcon(this NavigationViewModel model, NavigationNode node)
        {
            if (node == null) return string.Empty;
            if (string.IsNullOrEmpty(node.IconCssClass))
            {
                return string.Empty;

            }
            return "<i class='" + node.IconCssClass + "'></i> ";
        }

    }
}
