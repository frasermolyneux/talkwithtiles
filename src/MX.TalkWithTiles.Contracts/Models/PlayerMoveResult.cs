using System;
using System.Collections.Generic;

namespace MX.TalkWithTiles.Contracts.Models;

public class PlayerMoveResult(Guid playerId)
{
    public bool IsValid => string.IsNullOrWhiteSpace(InvalidMessage);

    public string? InvalidMessage { get; set; }
    public int Points { get; set; }
    public Guid PlayerId { get; set; } = playerId;
    public Guid NextPlayer { get; set; }
    public List<WordAndScore> WordsAndPoints { get; set; } = [];
    public List<Tile> Tiles { get; set; } = [];
}