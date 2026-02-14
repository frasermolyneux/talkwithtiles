using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Scrabble.Constants;

namespace MX.TalkWithTiles.CoreEngine.Helpers;

public static class StartingTileHelper
{
    public static IReadOnlyDictionary<string, int> GetStartingTiles(GameType gameType) => gameType switch
    {
        GameType.StandardBoard or GameType.SuperSizeBoard or GameType.MiniBoard => ScrabbleStartingTiles.Tiles[gameType],
        _ => throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null)
    };
}