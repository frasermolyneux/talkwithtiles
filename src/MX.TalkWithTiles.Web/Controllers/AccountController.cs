using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MX.TalkWithTiles.Web.Controllers;

[AllowAnonymous]
[Route("")]
public class AccountController(IWebHostEnvironment environment, IConfiguration configuration) : Controller
{
        [Route("sign-in")]
        public IActionResult SignIn(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(returnUrl ?? "/");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [Route("sign-in/microsoft")]
        public IActionResult SignInMicrosoft(string? returnUrl = null)
        {
            var redirectUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                "OpenIdConnect");
        }

        [Route("sign-out")]
        public new async Task<IActionResult> SignOut()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("SignIn");

            if (environment.IsDevelopment()
                && string.Equals(configuration["Testing:Enabled"], "true", StringComparison.OrdinalIgnoreCase))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Redirect("/");
            }

            return RedirectToAction("SignOut", "Account", new { area = "MicrosoftIdentity" });
        }
}
