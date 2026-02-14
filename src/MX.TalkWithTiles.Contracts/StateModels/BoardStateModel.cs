using MX.TalkWithTiles.Contracts.Models;

namespace MX.TalkWithTiles.Contracts.StateModels;

public class BoardStateModel
{
    public Tile[,]? Tiles { get; set; }
}