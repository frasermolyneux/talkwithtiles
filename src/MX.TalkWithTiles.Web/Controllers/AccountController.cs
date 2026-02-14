using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MX.TalkWithTiles.Web.Controllers;

[AllowAnonymous]
[Route("")]
public class AccountController : Controller
{
        [Route("sign-in")]
        public IActionResult SignIn(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Redirect(returnUrl ?? "/");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [Route("sign-in/microsoft")]
        public IActionResult SignInMicrosoft(string? returnUrl = null)
        {
            var redirectUrl = returnUrl ?? "/";
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                "OpenIdConnect");
        }

        [Route("sign-out")]
        public new IActionResult SignOut()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("SignIn");
            return RedirectToAction("SignOut", "Account", new { area = "MicrosoftIdentity" });
        }
}
