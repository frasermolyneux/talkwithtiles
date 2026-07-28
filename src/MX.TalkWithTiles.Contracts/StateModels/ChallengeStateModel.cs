using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Models;

namespace MX.TalkWithTiles.Contracts.StateModels;

public class ChallengeStateModel
{
    public bool CanOverrideChallengeOutcome { get; set; }
    public Dictionary<GameChallengeReason, GameChallengeResult> ChallengeResults { get; set; } = [];
    public Guid ChallengedPlayerId { get; set; }
    public Guid ChallengerPlayerId { get; set; }
    public GameChallengeReason? ChallengeReason { get; set; }
    public string? ChallengeText { get; set; }
    public PlayerChallengeResult? PlayerChallengeResult { get; set; }
}
