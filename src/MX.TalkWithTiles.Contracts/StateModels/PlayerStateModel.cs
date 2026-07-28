using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Models;

namespace MX.TalkWithTiles.Contracts.StateModels;

public class PlayerStateModel
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public List<Tile> Tiles { get; set; } = [];
    public int Score { get; set; }
    public List<Tile> NewTiles { get; set; } = [];
}
