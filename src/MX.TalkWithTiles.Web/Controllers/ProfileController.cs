using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MX.TalkWithTiles.Web.Extensions;

namespace MX.TalkWithTiles.Web.Controllers;

[Authorize]
[Route("profile")]
public class ProfileController : Controller
{
    [Route("")]
    public IActionResult Index()
    {
        ViewData["UserId"] = User.GetUserId();
        ViewData["UserName"] = User.GetUserName();
        ViewData["Email"] = User.GetEmail();

        return View();
    }
}
