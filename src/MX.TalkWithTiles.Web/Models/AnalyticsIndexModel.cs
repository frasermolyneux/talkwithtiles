using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Web.Models;

public class AnalyticsIndexModel
{
    public required List<GameStateModel> GameStates { get; set; }
}