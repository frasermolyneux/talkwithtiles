using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Models;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface ITileFactory
{
    Tile CreateTileForBag(string letter);
    Tile CreateTileForPosition(GameType gameType, int x, int y);
}