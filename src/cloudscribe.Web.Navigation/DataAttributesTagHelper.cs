// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2016-07-22
// Last Modified:			2016-07-22
// 

using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;

namespace cloudscribe.Web.Navigation
{
    [HtmlTargetElement(Attributes = MatchingAttributeName)]
    public class DataAttributesTagHelper : TagHelper
    {
        private const string MatchingAttributeName = "cwn-data-attributes";

        [HtmlAttributeName(MatchingAttributeName)]
        public List<DataAttribute> DataAttributes { get; set; } = null;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll(MatchingAttributeName);

            if ((DataAttributes != null)&&(DataAttributes.Count > 0))
            {
                foreach(var att in DataAttributes)
                {
                    output.Attributes.Add(att.Attribute, att.Value);
                }
            }

            
            
        }
    }
}
