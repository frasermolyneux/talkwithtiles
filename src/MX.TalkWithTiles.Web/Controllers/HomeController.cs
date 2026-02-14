using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MX.TalkWithTiles.Web.Extensions;
using MX.TalkWithTiles.Web.Models;

namespace MX.TalkWithTiles.Web.Controllers;

[Route("")]
public class HomeController(ILogger<HomeController> logger) : Controller
{
    [Route("")]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            this.AddAlertWarning(
                "Thanks for playing on Talk With Tiles. Feedback is encouraged through the feedback form <a href='/feedback'>here</a>");
        }

        return View();
    }

    [Route("privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [Route("cookies")]
    public IActionResult Cookies()
    {
        return View();
    }

    [Route("sitemap.xml")]
    public IActionResult Sitemap()
    {
        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                  "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">" +
                  "<url><loc>https://talkwithtiles.molyneux.me/</loc></url>" +
                  "<url><loc>https://talkwithtiles.molyneux.me/privacy</loc></url>" +
                  "<url><loc>https://talkwithtiles.molyneux.me/cookies</loc></url>" +
                  "<url><loc>https://talkwithtiles.molyneux.me/scrabble-games</loc></url>" +
                  "<url><loc>https://talkwithtiles.molyneux.me/getting-started</loc></url>" +
                  "<url><loc>https://talkwithtiles.molyneux.me/game-controls</loc></url>" +
                  "<url><loc>https://talkwithtiles.molyneux.me/faq</loc></url>" +
                  "</urlset>";
        return Content(xml, "text/xml");
    }

    [Route("feedback")]
    [Authorize]
    public IActionResult Feedback()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitFeedback(Feedback model)
    {
        if (!ModelState.IsValid)
            return View("Feedback", model);

        logger.LogInformation("User {User} has submitted feedback", User.GetUserName());

        this.AddAlertSuccess("Thanks for your feedback - we will take a look!");

        return RedirectToAction("Index");
    }
}