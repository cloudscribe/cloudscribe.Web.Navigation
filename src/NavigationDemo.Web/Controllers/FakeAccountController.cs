
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace NavigationDemo.Web.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Login(string userName = null)
        {
            //fake login with roles to demonstrate role based menu filtering
            AuthenticationProperties authProperties = new AuthenticationProperties();
            ClaimsPrincipal user;
            switch(userName)
            {
                case "Administrator":
                    user = GetAdminClaimsPrincipal();
                    break;

                case "Member":
                default:
                    user = GetMemberClaimsPrincipal();
                    break;
            }
            await HttpContext.Authentication.SignInAsync("application", user, authProperties);

            return RedirectToAction(nameof(HomeController.Index), "Home");


            //return View("Index");
        }

        private ClaimsPrincipal GetAdminClaimsPrincipal()
        {
            var identity = new ClaimsIdentity("application");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
            identity.AddClaim(new Claim(ClaimTypes.Name, "Administrator"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admins"));


            return new ClaimsPrincipal(identity);
        }

        private ClaimsPrincipal GetMemberClaimsPrincipal()
        {
            var identity = new ClaimsIdentity("application");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
            identity.AddClaim(new Claim(ClaimTypes.Name, "Member"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Members"));


            return new ClaimsPrincipal(identity);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            //await _signInManager.SignOutAsync();
            await HttpContext.Authentication.SignOutAsync("application");

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

    }
}
