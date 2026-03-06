using System;
using System.Collections.Generic;
using System.Linq;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.CoreEngine.Helpers;

namespace MX.TalkWithTiles.CoreEngine.Game;

public class BagManager(ITileFactory tileFactory) : IBagManager
{
    private readonly ITileFactory _tileFactory = tileFactory ?? throw new ArgumentNullException(nameof(tileFactory));

    private List<Tile> Tiles { get; set; } = [];

    public BagStateModel BagStateModel =>
        new()
        {
            Tiles = Tiles
        };

    public List<Tile> TakeTiles(int count)
    {
        List<Tile> tiles = [];

        for (var i = 0; i < count; i++)
        {
            if (Tiles.Count == 0)
                return tiles;

            var tile = Tiles[Random.Shared.Next(0, Tiles.Count)];
            tiles.Add(tile);
            Tiles.Remove(tile);
        }

        return tiles;
    }

    public void ReturnTilesToBag(List<Tile> tiles)
    {
        foreach (var bagTile in tiles.Select(tile => _tileFactory.CreateTileForBag(tile.Letter ?? string.Empty)))
            Tiles.Add(bagTile);
    }

    public void InitNew(GameType gameType)
    {
        Tiles = [];

        var availableTiles = StartingTileHelper.GetStartingTiles(gameType);
        foreach (var availableLetter in availableTiles)
            for (var i = 0; i < availableLetter.Value; i++)
            {
                var tile = _tileFactory.CreateTileForBag(availableLetter.Key);
                Tiles.Add(tile);
            }
    }

    public void InitFromStateModel(BagStateModel bagStateModel)
    {
        Tiles = bagStateModel.Tiles ?? [];
    }
}