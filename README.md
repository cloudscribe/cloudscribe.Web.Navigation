# cloudscribe.Web.Navigation
ASP.NET 5/MVC 6 solution for navigation menus and breadcrumbs.

[![Join the chat at https://gitter.im/joeaudette/cloudscribe](https://badges.gitter.im/joeaudette/cloudscribe.svg)](https://gitter.im/joeaudette/cloudscribe?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This was implemented in support of a larger project [cloudscribe.Core.Web](https://github.com/joeaudette/cloudscribe/) but has been moved to a separate repository since it has no dependencies on other "cloudscribe" components and should be useful in any ASP.NET 5/MVC 6 project that needs menus and breadcrumbs.

My early prototype work on cloudscribe.Core was done using ASP.NET 4.x/MVC 5, and at that time I was using [MVCSiteMapProvider](https://github.com/maartenba/MvcSiteMapProvider) for menus and breadcrumbs. When Visual Studio 2015RC was released with beta4 of ASP.NET 5/MVC 6, I began porting the cloudscribe project and [inquired whether ASP.NET 5 support was in the works for MVCSiteMapProvider](https://github.com/maartenba/MvcSiteMapProvider/issues/394). No work had begun in that direction and I needed a solution to move forward with cloudscribe so I decided to implement a replacement myself. I reviewed the MVCSiteMapProvider code but it seemed to me that it had a lot of code unrelated to navigation menus perhaps for legacy reasons. So I started from scratch to implement a minimal solution to meet the needs of my projects but not with the goal to provide every feature that was supported in MVCSiteMapProvider. I think it is fair to say that cloudscribe.Web.Navigation was inspired by MVCSiteMapProvider and it may meet the needs of some people who were using MVCSiteMapProvider and want to move to ASP.NET 5, but it is not based at all on the code for MVCSiteMapProvider. It would not surprise me if eventually the MVCSiteMapProvider team provides a better implementation for ASP.NET 5 than I have done.

This project is a work in progress and by no means completed yet. A few major things like caching and nested TreeBuilders are not completed yet. I will be updating the demo project to show new features and to better show existing features.

What is currently provided is a NavigationViewComponent which is used with various views to produce menus and breadcrumbs. The NavigationViewComponent depends on INavigationTreeBuilder to provide the tree of navigation nodes. We have an XmlNavigationTreeBuilder and a JsonNavigationTreeBuilder which can build the navigation tree from the corresponding file type. I find it easier to work with an xml file in this case because it is easier to find the beginning and end tag and comments are supported, one misplaced comma can easily break the json file and comments are not supported there.

I think when you have controllers and actions corresponding to the things you want navigation for, using an xml file is a nice easy way to wire up the menu. Menu nodes can be filtered from display by roles, so administrative or member only navigation items can be integrated into a holistic navigation tree.

The next major thing to implement that is not yet implemented is a way to nest implementations of INavigationTreeBuilder. What I want is to be able to start with an xml file for navigation using XmlNavigationTreeBuilder, but specify on a specific xml node to invoke another INavigationTreeBuilder to build a subtree of nodes onto the existing tree. This kind of thing would be needed for a CMS (Content Management System) for example where there may be many pages that all use the same controller and action. The navigation nodes for a CMS would be populated from a database. Then, of course I want the whole tree to be cached after building it.

Caching strategy is not fully baked yet either, and even with caching their are practical limits to consider. Caching the whole navigation tree should be a good solution for most sites with a reasonable number of navigation nodes, but probably if a site will have a million or more nodes (or some actual limit may be smaller) it will become problematic to build and cache such a large tree. At some point a strategy may be needed to split things into multiple smaller trees and only building and caching them as needed.

So just to reiterate this project is a work in progress and in early stages.

You can download/clone this repo and run the NavigationDemo.Web project to see a working example.

## Installation

Prerequisites:

*  [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads) 
*  [ASP.NET 5 RC1 Tooling](https://get.asp.net/) 

To install from nuget.org open the project.json file of your web application and in the dependencies section add:

    "cloudscribe.Web.Navigation": "1.0.0-*"
    
Visual Studio 2015 should restore the package automatically, you could also open a command prompt and use dnu restore in your project folder.

Unfortunately it is not yet possible for us to install the needed views from nuget. So for the moment you need to manually [copy the views folder and views from here](https://github.com/joeaudette/cloudscribe.Web.Navigation/tree/master/src/cloudscribe.Web.Navigation/content)

In your Startup.cs you will need this at the top: 

    using Microsoft.Framework.DependencyInjection.Extensions;
    using cloudscribe.Web.Navigation;

and in ConfigureServices you will need this:

    services.TryAddScoped<INavigationTreeBuilder, XmlNavigationTreeBuilder>();
    services.TryAddScoped<INodeUrlPrefixProvider, DefaultNodeUrlPrefixProvider>();
    services.TryAddScoped<INavigationNodePermissionResolver, NavigationNodePermissionResolver>();
    services.Configure<NavigationOptions>(Configuration.GetSection("NavigationOptions"));
    services.Configure<DistributedCacheNavigationTreeBuilderOptions>(Configuration.GetSection("DistributedCacheNavigationTreeBuilderOptions"));
    services.Configure<MemoryCacheNavigationTreeBuilderOptions>(Configuration.GetSection("MemoryCacheNavigationTreeBuilderOptions"));
    services.TryAddScoped<INavigationCacheKeyResolver, DefaultNavigationCacheKeyResolver>();

actually those last 3 items related to caching are not fully implemented yet

In your _ViewImports.cshtml file add:

    @using cloudscribe.Web.Navigation

You also need an navigation.xml file to define the navigation nodes. This can go in the root folder of your web app (not in wwwroot)

    <?xml version="1.0" encoding="utf-16"?>
    <NavNode key="Home" parentKey="RootNode" controller="Home" action="Index" text="Home" isRootNode="true">
      <Children>
        <NavNode key="About" parentKey="RootNode" controller="Home" action="About" text="About">
          <Children />
        </NavNode>
        <NavNode key="Contact" parentKey="RootNode" controller="Home" action="Contact" text="Contact">
          <Children />
        </NavNode>
        <NavNode key="Members" parentKey="RootNode" controller="Home" action="Members" text="Members" viewRoles="Admins,Members">
          <Children />
        </NavNode>
        <NavNode key="Administration" parentKey="RootNode" controller="Home" action="Administration" text="Administration" viewRoles="Admins">
          <Children />
        </NavNode>
      </Children>
    </NavNode>
    
Now you can use the ViewComponent in your views.

For example if you started with the standard ASP.NET 5 project template, you will have hard coded html navigation in the _Layout.cshtml file like this:

    <div class="navbar-collapse collapse">
        <ul class="nav navbar-nav">
            <li><a asp-controller="Home" asp-action="Index">Home</a></li>
            <li><a asp-controller="Home" asp-action="About">About</a></li>
            <li><a asp-controller="Home" asp-action="Contact">Contact</a></li>
        </ul>
        @await Html.PartialAsync("_LoginPartial")
    </div>
  
  You would replace that with this:
  
      <div class="navbar-collapse collapse">
          @await Component.InvokeAsync("Navigation", "BootstrapTopNav", NamedNavigationFilters.TopNav) 
          @await Html.PartialAsync("_LoginPartial")
      </div>
  
  That makes the top bootstrap navigation, now to add breadcrumbs put this in at the indicated spot:
  
      <div class="container body-content">
            @await Component.InvokeAsync("Navigation", "BootstrapBreadcrumbs", NamedNavigationFilters.Breadcrumbs)
            @RenderBody()

The div and the @RenderBody() should already be there, you just add the middle part that invokes the breadcrumbs.

The NavigationDemo.Web project has examples such as nodes filtered by roles, and a way to adjust breadcrumbs from a controller action. For now if you want to see a more advanced/detailed use of cloudscribe.Web.Navigation, you can study how it is being used in [cloudscribe.Core](https://github.com/joeaudette/cloudscribe).
