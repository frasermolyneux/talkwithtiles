using System;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IPlayerFactory
{
    IPlayer CreateNew(Guid playerId, string? playerName);
    IPlayer CreateFromStateModel(PlayerStateModel playerStateModel);
}