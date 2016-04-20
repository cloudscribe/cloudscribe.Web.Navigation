using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.Google;
using Microsoft.AspNet.Authentication.MicrosoftAccount;
using Microsoft.AspNet.Authentication.Twitter;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using cloudscribe.Web.Navigation;
using cloudscribe.Web.Navigation.Caching;
using cloudscribe.Web.SiteMap;
using Microsoft.AspNet.Http;
//using NavigationDemo.Web.Services;

namespace NavigationDemo.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // This reads the configuration keys from the secret store.
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // you can use either json or xml to maintain your navigation map we provide examples of each navigation.xml and 
            // navigation.json in the root of this project
            // you can override the name of the file used with AppSettings:NavigationXmlFileName or AppSettings:NavigationJsonFileName in config.json
            // the file must live in the root of the web project code not in wwwroot

            // it is arguable which is easier for humans to read and maintain, myself I think for something like a navigation tree
            // that could get large xml is easier to work with and not make mistakes. in json one missing or extra comma can break it
            // granted xml can be broken by typos too but the end tags make it easier to keep track of where you are imho (JA)
            //services.TryAddScoped<INavigationTreeBuilder, JsonNavigationTreeBuilder>();
            //services.TryAddScoped<INavigationTreeBuilder, HardCodedNavigationTreeBuilder>();

            // XmlNavigationTreeBuilder is the most tested implementation since it is the one I use

            services.TryAddScoped<ITreeCache, MemoryTreeCache>();
            services.TryAddScoped<INavigationTreeBuilder, XmlNavigationTreeBuilder>();
            services.AddScoped<NavigationTreeBuilderService, NavigationTreeBuilderService>();
            services.TryAddScoped<INodeUrlPrefixProvider, DefaultNodeUrlPrefixProvider>();
            services.TryAddScoped<INavigationNodePermissionResolver, NavigationNodePermissionResolver>();
            services.Configure<NavigationOptions>(Configuration.GetSection("NavigationOptions"));
            services.AddScoped<ISiteMapNodeService, NavigationTreeSiteMapNodeService>();

            services.Configure<MvcOptions>(options =>
            {
                // options.InputFormatters.Add(new Xm)
                options.CacheProfiles.Add("SiteMapCacheProfile",
                     new CacheProfile
                     {
                         Duration = 100
                     });

               

            });

            // Add MVC services to the services container.
            services.AddMvc();

            
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Configure the HTTP request pipeline.

            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // sends the request to the following path or controller action.
                app.UseExceptionHandler("/Home/Error");
            }

            // Add the platform handler to the request pipeline.
            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            
            var ApplicationCookie = new CookieAuthenticationOptions
            {
                AuthenticationScheme = "application",
                CookieName = "application",
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/FakeAccount/Index"),
                Events = new CookieAuthenticationEvents
                {
                    //OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                }
            };
            
            app.UseCookieAuthentication(ApplicationCookie);

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
