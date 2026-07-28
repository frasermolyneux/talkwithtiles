using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IPlayerManager
{
    PlayersStateModel PlayersStateModel { get; }
    List<IPlayer> Players { get; }
    void AddPlayer(Guid playerId, string? playerName);
    IPlayer GetPlayer(Guid playerId);
    List<IPlayer> GetPlayers();
    void SetTiles(Guid playerId, List<Tile> tiles);
    void AddToScore(Guid playerId, int score);
    void RemoveFromScore(Guid playerId, int score);
}
