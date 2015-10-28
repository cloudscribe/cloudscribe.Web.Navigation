using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;

namespace NavigationDemo.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        [Authorize(Roles = "Admins")]
        public IActionResult Administration()
        {
            ViewData["Message"] = "Administrators only.";

            return View();
        }

        [Authorize(Roles = "Admins,Members")]
        public IActionResult Members()
        {
            ViewData["Message"] = "Members only.";

            return View();
        }
    }
}
