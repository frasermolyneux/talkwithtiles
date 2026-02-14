using System;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Scrabble.Constants;

namespace MX.TalkWithTiles.CoreEngine.Helpers;

public static class BoardSizeHelper
{
    public static BoardSize GetSize(GameType gameType) => gameType switch
    {
        GameType.StandardBoard or GameType.SuperSizeBoard or GameType.MiniBoard => ScrabbleBoardSizes.Boards[gameType],
        _ => throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null)
    };
}