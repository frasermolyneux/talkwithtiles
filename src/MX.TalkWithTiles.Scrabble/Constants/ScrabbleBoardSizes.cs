using System.Collections.Frozen;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Models;

namespace MX.TalkWithTiles.Scrabble.Constants;

public static class ScrabbleBoardSizes
{
    public static readonly FrozenDictionary<GameType, BoardSize> Boards = new Dictionary<GameType, BoardSize>
    {
        {GameType.MiniBoard, new BoardSize(9, 9)},
        {GameType.StandardBoard, new BoardSize(15, 15)},
        {GameType.SuperSizeBoard, new BoardSize(19, 19)}
    }.ToFrozenDictionary();
}
