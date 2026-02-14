using System;
using System.Collections.Generic;
using System.Linq;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.CoreEngine.Game;

public class PlayerManager(IPlayerFactory playerFactory) : IPlayerManager
{
    public List<IPlayer> Players { get; private set; } = [];

    public PlayersStateModel PlayersStateModel => new()
    {
        Players = Players.Select(p => p.PlayerStateModel).ToList()
    };

    public void AddPlayer(Guid playerId, string? playerName)
    {
        var player = playerFactory.CreateNew(playerId, playerName);
        Players.Add(player);
    }

    public IPlayer GetPlayer(Guid playerId)
    {
        return Players.Single(player => player.PlayerId == playerId);
    }

    public List<IPlayer> GetPlayers()
    {
        return Players;
    }

    public void SetTiles(Guid playerId, List<Tile> tiles)
    {
        var player = GetPlayer(playerId);
        player.SetTiles(tiles);
    }

    public void AddToScore(Guid playerId, int score)
    {
        var player = GetPlayer(playerId);
        player.AddToScore(score);
    }

    public void RemoveFromScore(Guid playerId, int score)
    {
        var player = GetPlayer(playerId);
        player.RemoveFromScore(score);
    }

    public void InitNew()
    {
        Players = [];
    }

    public void InitFromStateModel(PlayersStateModel playersStateModel)
    {
        Players = [];

        foreach (var playerStateModel in playersStateModel.Players)
            Players.Add(playerFactory.CreateFromStateModel(playerStateModel));
    }
}