using cloudscribe.Web.Navigation;
using cloudscribe.Web.Navigation.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            services.TryAddScoped<ITreeCache, DistributedTreeCache>();

            services.TryAddScoped<INavigationTreeBuilder, XmlNavigationTreeBuilder>();
            services.AddScoped<INavigationTreeBuilder, ReflectionNavigationTreeBuilder>();
            services.TryAddScoped<NavigationTreeBuilderService, NavigationTreeBuilderService>();
            services.TryAddScoped<INodeUrlPrefixProvider, DefaultNodeUrlPrefixProvider>();
            services.TryAddScoped<INavigationNodePermissionResolver, NavigationNodePermissionResolver>();

            return services;
        }

        

    }  
}
