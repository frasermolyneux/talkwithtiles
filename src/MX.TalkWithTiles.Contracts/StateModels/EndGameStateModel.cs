using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Contracts.StateModels;

public class EndGameStateModel
{
    public GameStateType GameStateType { get; set; }
    public Dictionary<Guid, int> SkippedTurns { get; set; } = [];
    public List<Guid> Winners { get; set; } = [];
    public int WinnerPoints { get; set; }
    public Guid AbandoningPlayerId { get; set; }
}
