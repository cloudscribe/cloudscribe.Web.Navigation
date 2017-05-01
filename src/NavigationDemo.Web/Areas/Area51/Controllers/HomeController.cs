using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace NavigationDemo.Web.Areas.Area51.Controllers
{
    [Area("Area51")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Message"] = "Your area description page.";

            return View();
        }
    }
}
