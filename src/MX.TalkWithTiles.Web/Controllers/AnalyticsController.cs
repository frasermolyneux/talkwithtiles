using Microsoft.AspNetCore.Mvc;
using MX.TalkWithTiles.Web.Extensions;
using MX.TalkWithTiles.Repository.Interfaces;
using MX.TalkWithTiles.Repository.Models;
using MX.TalkWithTiles.Web.Models;
using MX.TalkWithTiles.Contracts.Constants;
using Microsoft.AspNetCore.Authorization;

namespace MX.TalkWithTiles.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AnalyticsController(IGameStateRepository gameStateRepository) : Controller
{
    public async Task<IActionResult> Index()
    {
        var gameStates = await gameStateRepository.GetGameStates(new GameStateFilterModel
        {
            GamePrivacyFilter = GamePrivacyType.All,
            Order = GameStateFilterModel.OrderBy.UpdatedDesc
        });

        var model = new AnalyticsIndexModel
        {
            GameStates = gameStates,
        };

        return View(model);
    }
}
