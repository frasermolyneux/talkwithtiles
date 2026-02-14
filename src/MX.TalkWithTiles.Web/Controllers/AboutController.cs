using Microsoft.AspNetCore.Mvc;

namespace MX.TalkWithTiles.Web.Controllers;

public class AboutController : Controller
{
        [Route("scrabble-games")]
        public IActionResult ScrabbleGames()
        {
            return View();
        }

        [Route("getting-started")]
        public IActionResult GettingStarted()
        {
            return View();
        }

        [Route("game-controls")]
        public IActionResult GameControls()
        {
            return View();
        }

        [Route("faq")]
        public IActionResult Faq()
        {
            return View();
        }
}