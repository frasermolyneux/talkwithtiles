using System;
using System.Collections.Generic;

namespace MX.TalkWithTiles.Contracts.Models;

public class PlayerMove
{
    public Guid PlayerId { get; set; }
    public List<Tile> Tiles { get; set; } = [];
}
