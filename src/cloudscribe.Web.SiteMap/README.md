# cloudscribe.Web.SiteMap
ASP.NET Core MVC controller and models for generating a [SiteMap for search engines](http://www.sitemaps.org/). Submitting a sitemap to the major search engines is vital to the [SEO](https://en.wikipedia.org/wiki/Search_engine_optimization) of most web sites.

[![Join the chat at https://gitter.im/joeaudette/cloudscribe](https://badges.gitter.im/joeaudette/cloudscribe.svg)](https://gitter.im/joeaudette/cloudscribe?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This was implemented in support of my [cloudscribe SimpleContent](https://github.com/joeaudette/cloudscribe.SimpleContent) project but has no dependencies on other cloudscribe projects and could be useful for many website projects.

What is currently provided is an mvc controller at /api/sitemap that can generate the xml sitemap from 1 or more IEnumerable<ISiteMapNode>. To use it in your own project you would implement ISiteMapNodeService which has one method, GetSiteMapNodes which returns an IEnumerable<ISiteMapNode>. You can register one or more implementations of ISiteMapNodeService with DI services and the controller will iterate through each of them to build the sitemap. There is also support for memory caching, but support for distributed cache has not been implemented yet.

Note that this project does not currently provide a sitemap solution for very large sites. A single sitemap which this project can generate dynamically, can only have up to 25,000 urls. If you have more that that you would need a differenet solution that generates a sitemap index which is a list of sitemap urls where each of the separate sitemaps can have up to 25,000 urls, and you can have up to 50,000 sitemaps listed in the index. I think if I were needing sitemaps that large I would create a process to generate the files as static xml files and serve those directly rather than generating the sitemap dynamically as in this project.


## Installation

Prerequisites:

*  [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads) 
*  [ASP.NET 5 RC1 Tooling](https://get.asp.net/) 

To install from nuget.org open the project.json file of your web application and in the dependencies section add:

    "cloudscribe.Web.SiteMap": "1.0.0-*"
    
Visual Studio 2015 should restore the package automatically, you could also open a command prompt and use dnu restore in your project folder.

In your Startup.cs you will need this at the top: 

    using Microsoft.Framework.DependencyInjection.Extensions;
    using cloudscribe.Web.SiteMap;

and in ConfigureServices you will need this:

    services.AddScoped<ISiteMapNodeService, YourImplementationOfSiteMapNodeService>();
	
You will also need to define a cache profile named SiteMapCacheProfile as part of the MVC configuration like this:

    services.Configure<MvcOptions>(options =>
	{
		options.CacheProfiles.Add("SiteMapCacheProfile",
			 new CacheProfile
			 {
				 Duration = 6000 //in seconds
			 });
	});
	services.AddMvc();
    

Follow me on twitter @cloudscribeweb and @joeaudette
