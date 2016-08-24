using cloudscribe.Web.Navigation.Caching;
using cloudscribe.Web.Navigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCloudscribeNavigation(
            this IServiceCollection services,
            IConfiguration configuration = null)
        {
            if(configuration != null)
            {
                //services.Configure<NavigationOptions>(configuration.GetSection("NavigationOptions"));
                services.Configure<NavigationOptions>(configuration);
                //services.AddSingleton<IConfigureOptions<NavigationOptions>>(new ConfigureFromConfigurationOptions<NavigationOptions>(configuration));
            }
            else
            {
                // does this add IOptions?
                services.TryAddSingleton<NavigationOptions, NavigationOptions>();
            }

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddScoped<ITreeCacheKeyResolver, DefaultCacheKeyResolver>();
            services.TryAddScoped<ITreeCache, MemoryTreeCache>();
            services.TryAddScoped<INavigationTreeBuilder, XmlNavigationTreeBuilder>();
            services.TryAddScoped<NavigationTreeBuilderService, NavigationTreeBuilderService>();
            services.TryAddScoped<INodeUrlPrefixProvider, DefaultNodeUrlPrefixProvider>();
            services.TryAddScoped<INavigationNodePermissionResolver, NavigationNodePermissionResolver>();

            return services;
        }



        /// <summary>
        /// This method adds an embedded file provider to the RazorViewOptions to be able to load the Navigation related views.
        /// If you download and install the views below your view folder you don't need this method and you can customize the views.
        /// You can get the views from https://github.com/joeaudette/cloudscribe.Web.Navigation/tree/master/src/cloudscribe.Web.Navigation/Views
        /// </summary>
        /// <param name="options"></param>
        /// <returns>RazorViewEngineOptions</returns>
        public static RazorViewEngineOptions AddEmbeddedViewsForNavigation(this RazorViewEngineOptions options)
        {
            options.FileProviders.Add(new EmbeddedFileProvider(
                    typeof(NavigationOptions).GetTypeInfo().Assembly,
                    "cloudscribe.Web.Navigation"
                ));

            return options;
        }



    }  
}
