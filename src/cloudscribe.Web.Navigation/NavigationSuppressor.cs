// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace cloudscribe.Web.Navigation
{
    /// <summary>
    /// Allows consuming applications to suppress specific navigation filters for the current request.
    /// When a filter is suppressed, the NavigationViewComponent and CachingNavigationViewComponent
    /// will return empty content immediately, avoiding all tree-building computation.
    /// </summary>
    public static class NavigationSuppressor
    {
        public static void SuppressFilter(HttpContext context, string filterName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrEmpty(filterName)) return;

            var key = Constants.NavigationSuppressContextKey;
            if (context.Items[key] is not HashSet<string> suppressed)
            {
                suppressed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                context.Items[key] = suppressed;
            }
            suppressed.Add(filterName);
        }

        public static bool IsFilterSuppressed(HttpContext context, string filterName)
        {
            if (context == null) return false;
            if (string.IsNullOrEmpty(filterName)) return false;

            if (context.Items[Constants.NavigationSuppressContextKey] is HashSet<string> suppressed)
            {
                return suppressed.Contains(filterName);
            }
            return false;
        }
    }
}
