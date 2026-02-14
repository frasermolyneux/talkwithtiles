using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IGameEngine
{
    GameStateModel GameStateModel { get; }
    void AddPlayer(Guid playerId, string? playerName);
    void SetRandomPlayerOrder();
    PlayerMoveResult MakeMove(PlayerMove playerMove, bool dryRun);
    void SkipMove(Guid playerId);
    void ExchangeTiles(Guid playerId, IEnumerable<Guid> tileIds);
    void AbandonGame(Guid playerId);
    void UpdateInvitedPlayer(Guid oldPlayerId, Guid newPlayerId, string newPlayerName);
    void IssuePlayerChallenge(Guid playerId, GameChallengeReason gameChallengeReason, string challengeText);
    void ResolveChallenge(bool accepted, GameChallengeResult? overrideChallengeResult);
    void UndoLastTurn(Guid playerId);
}