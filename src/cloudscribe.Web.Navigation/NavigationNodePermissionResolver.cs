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
            Assembly mvcProject = Assembly.GetEntryAssembly();
            var authAttrDict = new Dictionary<ActionKey, MvcAuthAttributes>();
            foreach (var controller in mvcProject.GetTypes().Where(typeof(Controller).IsAssignableFrom)) //filter controllers
            {
                TypeInfo ti = controller.GetTypeInfo();
                AuthorizeAttribute controllerAttribute = ti.GetCustomAttribute<AuthorizeAttribute>();
                string area = ti.GetCustomAttribute<AreaAttribute>()?.RouteValue;
                foreach(var action in ti.GetMethods().Where(action => action.DeclaringType == controller && action.IsPublic && !action.IsDefined(typeof(NonActionAttribute))))
                {
                    var httpMethodType = (from a in action.GetCustomAttributes()
                                          let t = a.GetType()
                                          where typeof(HttpMethodAttribute).IsAssignableFrom(t)
                                          select t).SingleOrDefault();
                    if (httpMethodType == null || httpMethodType == typeof(HttpGetAttribute))
                    {
                        const string controllerSuffix = "Controller";
                        var key = new ActionKey(area, controller.Name.EndsWith(controllerSuffix) ?controller.Name.Substring(0,controller.Name.Length - controllerSuffix.Length):controller.Name, action.Name);
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
            _authDictionary = new ReadOnlyDictionary<ActionKey, MvcAuthAttributes>(authAttrDict);
        }


        public NavigationNodePermissionResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        private IHttpContextAccessor _httpContextAccessor;

        private static ReadOnlyDictionary<ActionKey, MvcAuthAttributes> _authDictionary;
        public const string AllUsers = "AllUsers;"; //note  - shouldn't this be "*". At the very least, remove semicolon

        public virtual bool ShouldAllowView(TreeNode<NavigationNode> menuNode)
        {
            if (string.IsNullOrEmpty(menuNode.Value.ViewRoles)) {
                //use default comparison - that is, check if the action can allow the user
                var authAtts = GetRolesForAction(menuNode);
                return authAtts?.IsAuth(_httpContextAccessor.HttpContext.User) ?? true;
            }
            if (AllUsers.Equals(menuNode.Value.ViewRoles, StringComparison.OrdinalIgnoreCase)) { return true; }

            return _httpContextAccessor.HttpContext.User.IsInRoles(menuNode.Value.ViewRoles);
        }

        private static MvcAuthAttributes GetRolesForAction(TreeNode<NavigationNode> menuNode)
        {
            var key = new ActionKey(menuNode.Value.Area, menuNode.Value.Controller, menuNode.Value.Action);
            //todo - figure out how to calculate principal authorization by menuNode.Value.NamedRoute
            if (_authDictionary.TryGetValue(key, out MvcAuthAttributes attributes))
            {
                return attributes;
            }
            return null;
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

        private class ActionKey : Tuple<string,string,string>
        {
            public ActionKey(string area, string controller, string action) : base(area ?? string.Empty, controller, action)
            {
            }
        }
    }
}
