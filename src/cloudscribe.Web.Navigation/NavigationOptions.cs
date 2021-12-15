// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-09-05
// Last Modified:			2016-02-26
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation
{
    public class NavigationOptions
    {
        public NavigationOptions()
        { }

        public string RootTreeBuilderName { get; set; } = Constants.XmlNavigationTreeBuilderName;

        public string NavigationMapJsonFileName { get; set; } = "navigation.json";
        public string NavigationMapXmlFileName { get; set; } = "navigation.xml";

        /// <summary>
        /// Name of assemblies to scan NavNodeAttributes by reflection. 
        /// Leave it empty to disable this Configuration-by-Code feature.
        /// </summary>
        public string IncludeAssembliesForScan { get; set; }

        /// <summary>
        /// set to true to enable sorting. 
        /// If you use reflection, you'd better set this to true.
        /// for compatible reason, default is false.
        /// </summary>
        public bool EnableSorting { get; set; } = false;
    }
}
