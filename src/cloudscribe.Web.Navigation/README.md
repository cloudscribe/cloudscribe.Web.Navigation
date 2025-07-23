# cloudscribe.Web.Navigation

An ASP.NET Core viewcomponent for menus and breadcrumbs. Provides flexible, hierarchical navigation and breadcrumb support for web applications.

## Usage

1. Install the NuGet package:
   ```shell
   dotnet add package cloudscribe.Web.Navigation
   ```
2. Add the view component to your layout or page:
   ```csharp
   @await Component.InvokeAsync("Navigation")
   ```
3. Configure navigation in your app as needed. See the [GitHub repo](https://github.com/joeaudette/cloudscribe.Web.Navigation) for advanced usage and customization.

## License

Licensed under the Apache-2.0 License.
