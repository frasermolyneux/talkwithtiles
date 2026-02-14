using System;
using System.Collections.Generic;
using System.Linq;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.CoreEngine.Factories;
using MX.TalkWithTiles.CoreEngine.Game;
using MX.TalkWithTiles.CoreEngine.Helpers;
using Xunit;

namespace MX.TalkWithTiles.CoreEngine.Tests.Game;
public class BagManagerTests
{
    private readonly ITileFactory _tileFactory;

    public BagManagerTests()
    {
        _tileFactory = new TileFactory();
    }

    [Theory]
    [InlineData(GameType.MiniBoard)]
    [InlineData(GameType.StandardBoard)]
    [InlineData(GameType.SuperSizeBoard)]
    public void InitNewGeneratesTiles(GameType gameType)
    {
        // Arrange
        var bagManager = new BagManager(_tileFactory);

        // Act
        bagManager.InitNew(gameType);

        // Assert
        var result = bagManager.BagStateModel;

        Assert.NotNull(result);
        var expectedTileCount = StartingTileHelper.GetStartingTiles(gameType).Sum(i => i.Value);
        Assert.Equal(expectedTileCount, result.Tiles!.Count);
    }

    [Fact]
    public void InitFromStateModelLoadsTiles()
    {
        // Arrange
        var bagManager = new BagManager(_tileFactory);

        // Act
        var bagStateModel = new BagStateModel
        {
            Tiles = GenerateTiles()
        };
        bagManager.InitFromStateModel(bagStateModel);

        // Assert
        var result = bagManager.BagStateModel;

        Assert.NotNull(result);
        Assert.Equal(15, result.Tiles!.Count);
    }

    [Fact]
    public void ReturnTilesToBag()
    {
        // Arrange
        var bagManager = new BagManager(_tileFactory);

        var tilesToReturn = new List<Tile>
        {
            new()
            {
                Letter = "P",
                RackPosition = -1
            },
            new()
            {
                Letter = "Q",
                RackPosition = -1
            },
            new()
            {
                Letter = "R",
                RackPosition = -1
            }
        };

        // Act
        var bagStateModel = new BagStateModel
        {
            Tiles = GenerateTiles()
        };
        bagManager.InitFromStateModel(bagStateModel);
        bagManager.ReturnTilesToBag(tilesToReturn);

        // Assert
        var result = bagManager.BagStateModel;

        Assert.NotNull(result);
        Assert.Equal(18, result.Tiles!.Count);

        Assert.NotNull(result.Tiles.SingleOrDefault(i => i.Letter == "P"));
        Assert.NotNull(result.Tiles.SingleOrDefault(i => i.Letter == "Q"));
        Assert.NotNull(result.Tiles.SingleOrDefault(i => i.Letter == "R"));
    }

    private static List<Tile> GenerateTiles()
    {
        return new[] {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O"}.Select(i =>
            new Tile
            {
                TileId = Guid.NewGuid(),
                Letter = i
            }).ToList();
    }
}
