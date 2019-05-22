using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavigationDemo.Web.Controllers
{
    public class WhateverController : Controller
    {
        public WhateverController()
        {
            
        }

        public IActionResult Overview()
        {
            

            return View();
        }
    }
}
