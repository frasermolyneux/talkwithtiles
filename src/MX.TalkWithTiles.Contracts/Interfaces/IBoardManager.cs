using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IBoardManager
{
    BoardStateModel BoardStateModel { get; }
    PlayerMoveResult MakeMove(PlayerMove playerMove);
    void UndoMove(List<Tile> tiles);
    int LetterValue(string letter);
}