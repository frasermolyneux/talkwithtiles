using System;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IChallengeManager
{
    ChallengeStateModel ChallengeStateModel { get; }
    void IssuePlayerChallenge(Guid playerId, GameChallengeReason gameChallengeReason, string challengeText);
    void ResolveChallenge(bool accepted, GameChallengeResult? overrideChallengeResult);
}
