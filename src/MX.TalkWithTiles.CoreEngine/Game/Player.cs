using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.CoreEngine.Game;

public class Player : IPlayer
{
    public string? PlayerName { get; private set; }
    public List<Tile> NewTiles { get; private set; } = [];
    public List<Tile> Tiles { get; set; } = [];
    public Guid PlayerId { get; private set; }
    public int Score { get; set; }

    public PlayerStateModel PlayerStateModel => new()
    {
        PlayerId = PlayerId,
        PlayerName = PlayerName ?? string.Empty,
        Tiles = Tiles,
        Score = Score,
        NewTiles = NewTiles
    };

    public void SetTiles(List<Tile> tiles)
    {
        Tiles = tiles;

        for (var i = 0; i < tiles.Count; i++) Tiles[i].RackPosition = i;
    }

    public void AddToScore(int score)
    {
        Score += score;
    }

    public void UpdateInvitedPlayer(Guid newPlayerId, string newPlayerName)
    {
        PlayerId = newPlayerId;
        PlayerName = newPlayerName;
    }

    public void RemoveFromScore(int score)
    {
        Score -= score;
    }

    public void SetNewTiles(List<Tile> newTiles)
    {
        NewTiles = newTiles;
    }

    public void InitNew(Guid playerId, string? playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        Tiles = [];
        NewTiles = [];
    }

    public void InitFromStateModel(PlayerStateModel playerStateModel)
    {
        PlayerId = playerStateModel.PlayerId;
        PlayerName = playerStateModel.PlayerName;
        Tiles = playerStateModel.Tiles;
        Score = playerStateModel.Score;
        NewTiles = playerStateModel.NewTiles;
    }
}