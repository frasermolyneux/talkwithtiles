using System;
using System.Linq;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.CoreEngine.Helpers;

namespace MX.TalkWithTiles.CoreEngine.Factories;

public class TileFactory : ITileFactory
{
    public Tile CreateTileForBag(string letter)
    {
        return new Tile { Letter = letter, TileId = Guid.NewGuid() };
    }

    public Tile CreateTileForPosition(GameType gameType, int x, int y)
    {
        var boardTiles = BoardTileHelper.GetBoardTiles(gameType);

        if (boardTiles.Any(t => t.PosX == x && t.PosY == y))
            return boardTiles.Single(t => t.PosX == x && t.PosY == y);

        return new Tile
        {
            PosX = x,
            PosY = y,
            TileType = TileType.StandardTile
        };
    }
}