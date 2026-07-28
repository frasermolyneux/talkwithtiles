using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IPlayerMoveManager
{
    PlayerMoveStateModel PlayerMoveStateModel { get; }
    PlayerMoveResult? LastMoveResult { get; }
    void UndoLastTurn(Guid playerId);
    void ExchangeTiles(Guid playerId, IEnumerable<Guid> tileIds);
    void SkipTurn(Guid playerId);
    PlayerMoveResult MakeMove(PlayerMove playerMove, bool dryRun);
    void SetLastMovedToChallengeResolved();
    Guid SetNextPlayer();
    void SetRandomPlayerOrder();
    void UpdateInvitedPlayer(Guid oldPlayerId, Guid newPlayerId, string newPlayerName);
}
