using System;
using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Contracts.Models;

public class Tile
{
    public Guid TileId { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }

    public bool LetterSet => !string.IsNullOrWhiteSpace(Letter);

    public string? Letter { get; set; }
    public TileType TileType { get; set; }

    public int RackPosition { get; set; }
}