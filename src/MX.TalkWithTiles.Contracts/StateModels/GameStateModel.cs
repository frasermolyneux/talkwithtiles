using System;
using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Contracts.StateModels;

public class GameStateModel
{
    public Guid GameId { get; set; }
    public GamePrivacyType GamePrivacyType { get; set; }
    public GameType GameType { get; set; }
    public TileBagVisibilityOption TileBagVisibilityOption { get; set; }

    public string? GameEtag { get; set; }
    public DateTimeOffset LastUpdated { get; set; }

    public required BoardStateModel BoardStateModel { get; set; }
    public required BagStateModel BagStateModel { get; set; }
    public required PlayersStateModel PlayersStateModel { get; set; }
    public required EndGameStateModel EndGameStateModel { get; set; }
    public required ChallengeStateModel ChallengeStateModel { get; set; }
    public required PlayerMoveStateModel PlayerMoveStateModel { get; set; }
}
