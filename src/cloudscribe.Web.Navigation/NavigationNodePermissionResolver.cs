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
using System;
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
            //key = {namedRoute}/{area}/{controllername}
            _controllers = new ReadOnlyDictionary<string, Type>(asm.GetTypes()
                .Where(typeof(Controller).IsAssignableFrom) //filter controllers
                .ToDictionary(t =>
                {
                    TypeInfo ti = t.GetTypeInfo();
                    return ti.GetCustomAttribute<RouteAttribute>()?.Name.Trim(_routeTrimChars) + '/' + 
                        ti.GetCustomAttribute<AreaAttribute>()?.RouteValue + '/' +
                        (t.Name.EndsWith(controllerSuffix) ? t.Name.Substring(0, t.Name.Length - controllerSuffix.Length) : t.Name);
                }));
        }


        public NavigationNodePermissionResolver(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        private IHttpContextAccessor _httpContextAccessor;
        private static ReadOnlyDictionary<string, Type> _controllers;
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
            if (!_controllers.TryGetValue(menuNode.Value.NamedRoute.Trim(_routeTrimChars) +'/' + menuNode.Value.Area + '/' + menuNode.Value.Controller, out Type controller))
            {
                throw new Exception($"Could not find controller {menuNode.Value.Controller}");
            }
            var action = controller.GetMethod(menuNode.Value.Action);
            //potentially should exclude where action.IsDefined(typeof(NonActionAttribute)
            if (action == null)
            {
                throw new Exception($"Could not find action {menuNode.Value.Action} on controller {menuNode.Value.Controller}");
            }

            if (action.IsDefined(typeof(AllowAnonymousAttribute)))
            {
                return new MvcAuthAttributes();
            }

            return new MvcAuthAttributes
            {
                Controller = action.GetCustomAttribute<AuthorizeAttribute>(),
                Action = action.GetCustomAttribute<AuthorizeAttribute>()
            };
        }

        private class MvcAuthAttributes
        {
            public AuthorizeAttribute Controller { get; set; }
            public AuthorizeAttribute Action { get; set; }

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
