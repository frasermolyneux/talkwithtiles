using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IPlayer
{
    Guid PlayerId { get; }
    List<Tile> Tiles { get; }
    int Score { get; set; }
    PlayerStateModel PlayerStateModel { get; }
    List<Tile> NewTiles { get; }
    void SetTiles(List<Tile> tiles);
    void AddToScore(int score);
    void UpdateInvitedPlayer(Guid newPlayerId, string newPlayerName);
    void RemoveFromScore(int score);
    void SetNewTiles(List<Tile> newTiles);
}