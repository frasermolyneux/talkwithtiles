using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Scrabble.Constants;

namespace MX.TalkWithTiles.CoreEngine.Helpers;

public static class BoardTileHelper
{
    public static IReadOnlyList<Tile> GetBoardTiles(GameType gameType) => gameType switch
    {
        GameType.StandardBoard or GameType.SuperSizeBoard or GameType.MiniBoard => ScrabbleBoardTiles.Tiles[gameType],
        _ => throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null)
    };
}