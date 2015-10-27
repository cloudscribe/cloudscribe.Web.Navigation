
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;


namespace NavigationDemo.Web.Controllers
{
    public class FakeAccountController : Controller
    {
        public FakeAccountController()
        {

        }

        // GET: /Account/index
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewData["Title"] = "Login";
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string userName = null)
        {
            //TODO: fake login with roles to demonstrate role based menu filtering


            return View("Index");
        }

    }
}
