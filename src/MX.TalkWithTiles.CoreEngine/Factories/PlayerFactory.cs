using System;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.CoreEngine.Game;

namespace MX.TalkWithTiles.CoreEngine.Factories;

public class PlayerFactory : IPlayerFactory
{
    public IPlayer CreateNew(Guid playerId, string? playerName)
    {
        var player = new Player();
        player.InitNew(playerId, playerName);
        return player;
    }

    public IPlayer CreateFromStateModel(PlayerStateModel playerStateModel)
    {
        var player = new Player();
        player.InitFromStateModel(playerStateModel);
        return player;
    }
}