using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavigationDemo.Web.Areas.Area51.Controllers
{
    [Area("Area51")]
    public class RoswellController : Controller
    {
        public RoswellController()
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Aliens()
        {
            return View();
        }

        public IActionResult MenInBlack()
        {
            return View();
        }

    }
}
