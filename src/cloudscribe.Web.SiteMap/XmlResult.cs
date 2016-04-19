using Microsoft.AspNet.Mvc;
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

        public override void ExecuteResult(ActionContext context)
        { 
            context.HttpContext.Response.ContentType = this.ContentType;
            if (Xml != null)
            {
                Xml.Save(context.HttpContext.Response.Body, SaveOptions.DisableFormatting);
            }
        }
        

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.ContentType = this.ContentType;

            if (Xml != null)
            {
                Xml.Save(context.HttpContext.Response.Body, SaveOptions.DisableFormatting);
                return Task.FromResult(0);

            }
            else
            {
                return base.ExecuteResultAsync(context);
            }
        }
    }
}
