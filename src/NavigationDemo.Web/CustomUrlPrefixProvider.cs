using cloudscribe.Web.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavigationDemo.Web
{
    public class CustomUrlPrefixProvider : INodeUrlPrefixProvider
    {
        public CustomUrlPrefixProvider()
        { }

        public string GetPrefix()
        {
            return "/test/cloudscribe";
        }
    }
}
