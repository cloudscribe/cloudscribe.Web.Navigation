using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavigationDemo.Web.Controllers
{
    public class OverviewController : Controller
    {
        public OverviewController()
        {

        }

        public IActionResult Index()
        {


            return View();
        }
    }
}
