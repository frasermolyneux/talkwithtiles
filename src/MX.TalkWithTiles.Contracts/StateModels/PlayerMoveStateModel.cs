using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Models;

namespace MX.TalkWithTiles.Contracts.StateModels;

public class PlayerMoveStateModel
{
    public Guid CurrentPlayerId { get; set; }
    public List<Guid> PlayerOrderIds { get; set; } = [];
    public int TurnsTaken { get; set; }
    public LastMoveType LastMoveType { get; set; }
    public PlayerMoveResult? LastMoveResult { get; set; }
}