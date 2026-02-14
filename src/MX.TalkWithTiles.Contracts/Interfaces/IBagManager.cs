using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IBagManager
{
    BagStateModel BagStateModel { get; }
    List<Tile> TakeTiles(int count);
    void ReturnTilesToBag(List<Tile> tiles);
}