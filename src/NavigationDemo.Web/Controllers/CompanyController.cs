using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavigationDemo.Web.Controllers
{
    public class CompanyController : Controller
    {

        public IActionResult Index()
        {


            return View();
        }

        public IActionResult Services()
        {


            return View();
        }


    }
}
