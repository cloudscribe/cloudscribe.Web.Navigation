using cloudscribe.Web.Navigation.Caching;
using cloudscribe.Web.Navigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

//namespace cloudscribe.Web.Navigation
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCloudscribeNavigation(
            this IServiceCollection services,
            IConfigurationRoot configuration = null)
        {
            if(configuration != null)
            {
                services.Configure<NavigationOptions>(configuration.GetSection("NavigationOptions"));
            }
            else
            {
                services.TryAddSingleton<NavigationOptions, NavigationOptions>();
            }

            services.TryAddScoped<ITreeCache, MemoryTreeCache>();
            services.TryAddScoped<INavigationTreeBuilder, XmlNavigationTreeBuilder>();
            services.TryAddScoped<NavigationTreeBuilderService, NavigationTreeBuilderService>();
            services.TryAddScoped<INodeUrlPrefixProvider, DefaultNodeUrlPrefixProvider>();
            services.TryAddScoped<INavigationNodePermissionResolver, NavigationNodePermissionResolver>();

            return services;
        }
    }
}
