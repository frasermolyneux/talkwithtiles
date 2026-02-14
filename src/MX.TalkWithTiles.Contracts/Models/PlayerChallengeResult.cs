using System;
using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Contracts.Models;

public class PlayerChallengeResult
{
    public Guid ChallengedPlayerId { get; set; }
    public Guid ChallengerPlayerId { get; set; }
    public GameChallengeReason GameChallengeReason { get; set; }
    public GameChallengeResult GameChallengeResult { get; set; }
}