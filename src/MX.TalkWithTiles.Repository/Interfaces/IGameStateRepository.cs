using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.Repository.Models;

namespace MX.TalkWithTiles.Repository.Interfaces;

public interface IGameStateRepository
{
    Task<GameStateModel?> GetGameState(Guid gameId, bool skipTileFetch = false);
    Task UpdateGameState(Guid gameId, GameStateModel gameStateModel);
    Task<List<GameStateModel>> GetGameStates(GameStateFilterModel filterModel);
    Task DeleteGameStateIndex(Guid id, Guid playerId);
}