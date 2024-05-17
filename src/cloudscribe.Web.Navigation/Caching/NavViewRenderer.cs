using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace cloudscribe.Web.Navigation.Caching
{
    /// <summary>
    /// produce an html string representing the site nav - for use incaching - using 
    /// Razor templates and models
    /// 
    /// JimK - this is based on the main CS version, but simplified - 
    /// passing in to it the actionContext etc etc. from the consuming method 
    /// rather than relying on DI services in here
    /// seems to prevent a proliferation of "object disposed" errors.
    /// </summary>
    public class NavViewRenderer
    {
        public NavViewRenderer(ILogger<NavViewRenderer> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<NavViewRenderer> _logger;
        
        public async Task<string> RenderViewAsStringWithActionContext<TModel>(string             viewName, 
                                                                              TModel             model, 
                                                                              ViewEngineResult   viewResult, 
                                                                              ActionContext      actionContext, 
                                                                              TempDataDictionary tempData)
        {
            var viewData = new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
            {
                Model = model
            };


            try
            {
                using (StringWriter output = new StringWriter())
                {
                    ViewContext viewContext = new ViewContext(
                        actionContext,
                        viewResult.View,
                        viewData,
                        tempData,
                        output,
                        new HtmlHelperOptions()
                    );

                    await viewResult.View.RenderAsync(viewContext);

                    return output.GetStringBuilder().ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NavViewRenderer - error in view rendering for view " + viewName);
                throw ex;
            }
        }
    }
}
