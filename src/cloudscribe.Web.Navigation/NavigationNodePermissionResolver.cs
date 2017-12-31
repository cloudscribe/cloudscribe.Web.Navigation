// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-07-11
// Last Modified:			2016-05-17
// 

using cloudscribe.Web.Navigation.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

namespace cloudscribe.Web.Navigation
{
    public class NavigationNodePermissionResolver : INavigationNodePermissionResolver
    {
        public NavigationNodePermissionResolver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            ILogger<NavigationNodePermissionResolver> logger
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _log = logger;
        }

        private IHttpContextAccessor _httpContextAccessor;
        private IAuthorizationService _authorizationService;
        private ILogger _log;

        public virtual bool ShouldAllowView(TreeNode<NavigationNode> menuNode)
        {
            if(!string.IsNullOrEmpty(menuNode.Value.AuthorizationPolicy))
            {
                try
                {
                    var authResult = _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, menuNode.Value.AuthorizationPolicy).GetAwaiter().GetResult();
                    return authResult.Succeeded;
                }
                catch(InvalidOperationException ex)
                {
                    _log.LogError($"InvalidOperation exception thrown when trying to authorize node with key {menuNode.Value.Key} which has authorization policy named {menuNode.Value.AuthorizationPolicy}, the error was {ex.Message}");
                    return false;
                }
                
            }

            if (string.IsNullOrEmpty(menuNode.Value.ViewRoles)) { return true; }
            if (menuNode.Value.ViewRoles == "All Users;") { return true; }

            if (_httpContextAccessor.HttpContext.User.IsInRoles(menuNode.Value.ViewRoles))
            {
                return true;
            }

            return false;
        }
    }
}
