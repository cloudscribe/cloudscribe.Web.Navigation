// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-11
// Last Modified:			2016-05-17
// 

using cloudscribe.Web.Navigation.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace cloudscribe.Web.Navigation
{
    public class NavigationNodePermissionResolver : INavigationNodePermissionResolver
    {
        static NavigationNodePermissionResolver()
        {
            //this may throw sytem notsupported exception on Mac & Linux???
            //in this format the great problem will be testability - what if the entry assembly is a test
            //better to use injection in later iterations beyond current 'proof of concept'
            Assembly asm = Assembly.GetEntryAssembly();
            const string controllerSuffix = "Controller";
            _routeTrimChars = new[] { '/', ' ' };
            var authAttrDict = new Dictionary<string, MvcAuthAttributes>();
            foreach (var controller in asm.GetTypes().Where(typeof(Controller).IsAssignableFrom)) //filter controllers
            {
                TypeInfo ti = controller.GetTypeInfo();
                AuthorizeAttribute controllerAttribute = ti.GetCustomAttribute<AuthorizeAttribute>();
                string keyPrefix = ti.GetCustomAttribute<RouteAttribute>()?.Name.Trim(_routeTrimChars) + '/' + 
                    ti.GetCustomAttribute<AreaAttribute>()?.RouteValue + '/' +
                    (controller.Name.EndsWith(controllerSuffix)
                        ? controller.Name.Substring(0, controller.Name.Length - controllerSuffix.Length) 
                        : controller.Name) + '/';
                foreach(var action in ti.GetMethods().Where(action => action.IsPublic && !action.IsDefined(typeof(NonActionAttribute))))
                {
                    var httpMethodType = (from a in action.GetCustomAttributes()
                                          let t = a.GetType()
                                          where typeof(HttpMethodAttribute).IsAssignableFrom(t)
                                          select t).SingleOrDefault();
                    if (httpMethodType == null || httpMethodType == typeof(HttpGetAttribute))
                    {
                        string key = keyPrefix + action.Name;
                        if (action.IsDefined(typeof(AllowAnonymousAttribute)))
                        {
                            authAttrDict.Add(key, new MvcAuthAttributes(null, null));
                        }
                        else
                        {
                            authAttrDict.Add(key, new MvcAuthAttributes(
                                controllerAttribute,
                                action.GetCustomAttribute<AuthorizeAttribute>()));
                        }
                    }
                }
            }
            _controllers = new ReadOnlyDictionary<string, MvcAuthAttributes>(authAttrDict);
        }


        public NavigationNodePermissionResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private IHttpContextAccessor _httpContextAccessor;
        //key = {namedRoute}/{area}/{controllerName}/{actionName}
        private static ReadOnlyDictionary<string, MvcAuthAttributes> _controllers;
        private static readonly char[] _routeTrimChars;
        public const string AllUsers = "AllUsers;"; //note  - shouldn't this be "*". At the very least, remove semicolon

        public virtual bool ShouldAllowView(TreeNode<NavigationNode> menuNode)
        {
            if (string.IsNullOrEmpty(menuNode.Value.ViewRoles)) {
                //use default comparison - that is, check if the action can allow the user
                var authAtts = GetRolesForAction(menuNode);
                return authAtts.IsAuth(_httpContextAccessor.HttpContext.User);
            }
            if (AllUsers.Equals(menuNode.Value.ViewRoles, StringComparison.OrdinalIgnoreCase)) { return true; }

            return _httpContextAccessor.HttpContext.User.IsInRoles(menuNode.Value.ViewRoles);
        }

        private static MvcAuthAttributes GetRolesForAction(TreeNode<NavigationNode> menuNode)
        {
            if (!_controllers.TryGetValue(menuNode.Value.NamedRoute.Trim(_routeTrimChars) +'/' + menuNode.Value.Area + '/' + menuNode.Value.Controller + '/' + menuNode.Value.Action, out MvcAuthAttributes attributes))
            {
                throw new Exception($"Could not find action {menuNode.Value.Action} on controller {menuNode.Value.Controller}");
            }
            return attributes;
        }

        private class MvcAuthAttributes
        {
            public MvcAuthAttributes(AuthorizeAttribute controller, AuthorizeAttribute action)
            {
                Controller = controller;
                Action = action;
            }
            public AuthorizeAttribute Controller { get; private set; }
            public AuthorizeAttribute Action { get; private set; }

            public bool IsAuth(System.Security.Claims.ClaimsPrincipal principal)
            {
                return IsAuth(Controller, principal) && IsAuth(Action, principal);
            }
            private static bool IsAuth(AuthorizeAttribute attr, System.Security.Claims.ClaimsPrincipal principal)
            {
                if (attr == null)
                {
                    return true;
                }
                if (string.IsNullOrEmpty(attr.Roles))
                {
                    return principal.Identity.IsAuthenticated;
                }
                return principal.IsInRoles(attr.Roles);
            }
        }
    }
}
