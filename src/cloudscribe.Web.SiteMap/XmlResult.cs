using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace cloudscribe.Web.SiteMap
{
    /// <summary>
    /// I've used this code in at least 3 projects so far which sort of makes me want to just make it its own nuget
    /// but it is so little code that seems overkill and just adding another dependency
    /// re-use and a common place to fix it if something needs changing would be ideal 
    /// but less dependencies is also a good thing so I'm torn
    /// would be interested in other's opinions on this matter
    /// in the node world small packages are considered fine and good practice
    /// even if it did blow up in their face recently http://www.haneycodes.net/npm-left-pad-have-we-forgotten-how-to-program/
    /// </summary>
    public class XmlResult : ActionResult
    {
        public XDocument Xml { get; private set; }
        public string ContentType { get; set; }
        
        public XmlResult(XDocument xml)
        {
            this.Xml = xml;
            this.ContentType = "text/xml";
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.ContentType = this.ContentType;

            if (Xml != null)
            {
                try 
                {
                    // refactor to avoid IO issue below
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await Xml.SaveAsync(ms, SaveOptions.DisableFormatting, default(CancellationToken));
                        ms.Seek(0, SeekOrigin.Begin);
                        await ms.CopyToAsync(context.HttpContext.Response.Body, default(CancellationToken));
                    }
                }
                catch
                {
                    // synchronous IO disabled in Core 3.0
                    // https://khalidabuhakmeh.com/dotnet-core-3-dot-0-allowsynchronousio-workaround
                    // https://github.com/dotnet/aspnetcore/issues/7644

                    // workaround:
                    var syncIOFeature = context.HttpContext.Features.Get<IHttpBodyControlFeature>();
                    if (syncIOFeature != null)
                    {
                        syncIOFeature.AllowSynchronousIO = true;
                    }

                    // old way of doing this triggered IO exception:
                    await Xml.SaveAsync(context.HttpContext.Response.Body, SaveOptions.DisableFormatting, CancellationToken.None);
                }
            }
            else
            {
               await base.ExecuteResultAsync(context);
            }
        }
    }
}
