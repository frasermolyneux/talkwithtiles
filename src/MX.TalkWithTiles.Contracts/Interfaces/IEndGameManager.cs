using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IEndGameManager
{
    EndGameStateModel EndGameStateModel { get; }
    void SetWinners(List<Guid> playerIds, int winningScore);
    void AbandonGame(Guid playerId);
}
