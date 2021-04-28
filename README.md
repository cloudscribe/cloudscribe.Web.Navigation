This is a fork from [cloudscribe.Web.Navigation](https://github.com/cloudscribe/cloudscribe.Web.Navigation). 

I've made the following changes to smooth the upgrading work from ASP.net 4.x to ASP.net Core for my projects using [MvcSiteMapProvider](https://github.com/maartenba/MvcSiteMapProvider/).

# Background

We were using [MvcSiteMapProvider](https://github.com/maartenba/MvcSiteMapProvider/) for navigation(menus/breadcrumbs) in our .net 4.x project heavily. When planning to move to .net Core, we found it was missing .net Core support yet.

From [this discussion](https://github.com/maartenba/MvcSiteMapProvider/issues/394), we learnt the cloudscribe.Web.Navigation project. 

In our project, we used the [configuration-by-code feature](https://github.com/maartenba/MvcSiteMapProvider/wiki/Defining-sitemap-nodes-using-.NET-attributes) a lot. However cloudscribe.Web.Navigation did not support this feature. So this is the first reason for we made this fork.

# 1. [NavNodeAttribute]

This attribute is just like the [MvcSiteMapNodeAttribute], which makes it convenient to keep the node information in the same place as your controller action.

    [NavNode(Key = "ViewPage", ParentKey = "ProductIndexPage", Text = "View page")]
    public IActionResult View()
    {
        return View();
    }

Comparing with navigation.xml/json, 
* the field information of `controller/action/area` of the node will be collected by reflection automatically;
* other field information can be set via this attribute by code;
* `Key` is optional if it has no children node (see #2);
* `ParentKey` points to its parent node and will be used to build the navigation tree. If empty, it refers to the RootNode;
* Don't set duplicated keys and the RootNode should be only one.
* You may set `Order` to adjust the order of the nodes (see #3).

# 2. Key auto-generated

The name of keys are insignificant for most nodes. So we change it optional and generate a random key automatically if you don't set one.

This is not for [NavNode] only. It also works for the navigation.xml or navigation.json.

# 3. Node order

We add `Order` property to NavigationNode. You may adjust the display order of the nodes. 

Considering compatibilty, this feature is NOT enabled by default. (see #4)

# 4. Configuration

In appsettings.json, you may configure like below:

    {
        "NavigationOptions": {
            "RootTreeBuilderName": "cloudscribe.Web.Navigation.ReflectionNavigationTreeBuilder",
            "IncludeAssembliesForScan": "NavigationDemo.Web",
            "EnableSorting": true
        }
    }

You should put the assembly names into `IncludeAssembliesForScan` (comma-separated if two or more), and you should set `EnableSorting` to true.

The RootNode of the navigation tree should be marked like below:

    [NavNode(Key = "HomePage", ParentKey = "", Text = "Home page")]
    public IActionResult Index()
    {
        return View();
    }

# 5. Mixing configuration

You can also use both navigation.xml and [NavNode] (or both navigation.json and [NavNode]). For example in appsettings.json,

    {
        "NavigationOptions": {
            "RootTreeBuilderName": "cloudscribe.Web.Navigation.XmlNavigationTreeBuilder",
            "NavigationMapXmlFileName": "navigation.xml",
            "IncludeAssembliesForScan": "NavigationDemo.Web",
            "EnableSorting": true
        }
    }

It will load the navigation.xml configuration first, and then load the [NavNode] configuration. Of course, in this situation, the RootNode should be set in the navigation.xml.

# 6. Processing '$resources:....'

MvcSiteMapProvider supports an old localization feature, [which inherited from ASP.net Site Navigation](https://docs.microsoft.com/en-us/previous-versions/aspnet/ms178427(v=vs.100)?redirectedfrom=MSDN). You may use text in format: `$resources:ClassName,KeyName,DefaultValue` for `Title` or `Text` of the node.

# 7. Converting from 'Mvc.sitemap'

We also made a small console tool for converting from 'Mvc.sitemap' to navigation.xml.

# 8. KeyPrefix

We use some inherited controllers, and made this prefix feature. For example,

	public class ProductController : Controller
	{
		[NavNode(Key = "{Prefix}ProductList", Text = "List of Products")]
		public IActionResult List() {}
		[NavNode(ParentKey = "{Prefix}ProductList", Text = "Product details", PreservedRouteParameters = "id")]
		public IActionResult View(int id) {}
	}
	[Area("Staff")]
	[NavNodeController(KeyPrefix = "Staff")]
	public class StaffProductController : ProductController
	{
	}

* For `ProductController`, `List` node will has key: `ProductList` (prefix is empty now); `View` node will has parentKey `ProductList`, pointing to `List` action of the same controller;
* `StaffProductController` inherits from `ProductController`. `List` node will has key `StaffProductList` (prefix is `Staff` now); `View` node will has parentKey `StaffProductList`, pointing to `List` action of the same controller.


I'll pull this work to [cloudscribe.Web.Navigation](https://github.com/cloudscribe/cloudscribe.Web.Navigation) later. If accepted and merged, this fork will stop maintainance.
