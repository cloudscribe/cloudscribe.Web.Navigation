using cloudscribe.Web.Navigation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavigationDemo.Web.Controllers
{
    public class ReflectionController : Controller
    {
        [NavNode(Key = "{Prefix}ReflectionIndex", ParentKey = "{Prefix}Home", Order = -9,
            Text = "Reflection", ResourceType = typeof(Resources.MyResource))]
        public virtual IActionResult Index()
        {
            return View();
        }

        [NavNode(Key = "{Prefix}ReflectionMulan", ParentKey = "{Prefix}ReflectionIndex", 
            Text = "Mulan", ResourceType = typeof(Resources.MyResource))]
        public IActionResult Mulan()
        {
            return View();
        }

        [NavNode(Key = "{Prefix}ReflectionMushu", ParentKey = "{Prefix}ReflectionMulan", 
            Text = "Mushu", ResourceType = typeof(Resources.MyResource))]
        public IActionResult Mushu()
        {
            return View();
        }


    }
}
