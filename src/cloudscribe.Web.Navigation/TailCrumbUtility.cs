// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2017-05-02
// Last Modified:			2017-05-02
// 

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace cloudscribe.Web.Navigation
{
    public class TailCrumbUtility
    {
        public TailCrumbUtility(HttpContext context)
        {
            this.context = context ?? throw new ArgumentNullException("context");
        }

        private HttpContext context;

        public void AddTailCrumb(string key, string text, string url = "")
        {
            List<NavigationNode> tailCrumbs;
            bool found = false;

            if (context.Items[Constants.TailCrumbsContexctKey] == null)
            {
                tailCrumbs = new List<NavigationNode>();
            }
            else
            {
                found = true;
                tailCrumbs = (List<NavigationNode>)context.Items[Constants.TailCrumbsContexctKey];
            }

            var node = new NavigationNode()
            {
                Key = key,
                Text = text,
                Url = url
            };
            tailCrumbs.Add(node);

            if(!found)
            {
                context.Items[Constants.TailCrumbsContexctKey] = tailCrumbs;
            }

        }
    }
}
