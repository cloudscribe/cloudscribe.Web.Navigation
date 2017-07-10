using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;

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

        public IActionResult AboutMe()
        {
            ViewData["Message"] = "about me page.";

            return View("About");
        }

        public IActionResult AboutCompany()
        {
            ViewData["Message"] = "about company page.";

            return View("About");
        }

        public IActionResult AboutProject()
        {
            ViewData["Message"] = "about project page.";

            return View("About");
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult HideAuth()
        {
            ViewData["Message"] = "Your contact page.";

            return View("Contact");
        }

        public IActionResult HideAnon()
        {
            ViewData["Message"] = "Your contact page.";

            return View("Contact");
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

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public IActionResult ClearLanguageCookie(string culture, string returnUrl)
        {
            Response.Cookies.Delete(
                CookieRequestCultureProvider.DefaultCookieName
            );

            return LocalRedirect(returnUrl);
        }
    }
}
