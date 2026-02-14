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

    public BoardStateModel BoardStateModel { get; set; } = null!;
    public BagStateModel BagStateModel { get; set; } = null!;
    public PlayersStateModel PlayersStateModel { get; set; } = null!;
    public EndGameStateModel EndGameStateModel { get; set; } = null!;
    public ChallengeStateModel ChallengeStateModel { get; set; } = null!;
    public PlayerMoveStateModel PlayerMoveStateModel { get; set; } = null!;
}