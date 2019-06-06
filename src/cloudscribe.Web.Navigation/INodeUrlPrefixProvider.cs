// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-08-02
// Last Modified:			2019-06-06
// 


using Microsoft.AspNetCore.Http;

namespace cloudscribe.Web.Navigation
{
    public interface INodeUrlPrefixProvider
    {
        string GetPrefix();
    }

    public class DefaultNodeUrlPrefixProvider : INodeUrlPrefixProvider
    {
        public DefaultNodeUrlPrefixProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContentAccessor = httpContextAccessor;
        }

        private readonly IHttpContextAccessor _httpContentAccessor;

        public string GetPrefix()
        {
            if(_httpContentAccessor.HttpContext != null)
            {
                return _httpContentAccessor.HttpContext.Request.PathBase;
            }

            return string.Empty;
        }
    }
}
