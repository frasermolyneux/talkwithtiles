using System;
using System.Collections.Generic;
using Moq;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.Scrabble.Constants;
using Xunit;

namespace MX.TalkWithTiles.Scrabble.Tests;
public class ScrabbleBoardManagerTests
{
    private readonly Mock<ITileFactory> _mockTileFactory;

    public ScrabbleBoardManagerTests()
    {
        _mockTileFactory = new Mock<ITileFactory>();
    }

    [Theory]
    [InlineData(GameType.MiniBoard)]
    [InlineData(GameType.StandardBoard)]
    [InlineData(GameType.SuperSizeBoard)]
    public void InitNewGeneratesBoard(GameType gameType)
    {
        // Arrange
        var scrabbleBoardManager = new ScrabbleBoardManager(_mockTileFactory.Object);

        // Act
        scrabbleBoardManager.InitNew(gameType);

        // Assert
        var result = scrabbleBoardManager.BoardStateModel;

        Assert.NotNull(result);
        Assert.NotNull(result.Tiles);

        var expectedTileCount =
            ScrabbleBoardSizes.Boards[gameType].Width * ScrabbleBoardSizes.Boards[gameType].Height;
        Assert.Equal(expectedTileCount, result.Tiles.Length);
    }

    [Theory]
    [InlineData(GameType.MiniBoard)]
    [InlineData(GameType.StandardBoard)]
    [InlineData(GameType.SuperSizeBoard)]
    public void InitFromStateModelLoadsBoard(GameType gameType)
    {
        // Arrange
        var scrabbleBoardManager = new ScrabbleBoardManager(_mockTileFactory.Object);

        // Act
        var boardTiles = GenerateBoardTiles(gameType);

        var boardStateModel = new BoardStateModel
        {
            Tiles = boardTiles
        };
        scrabbleBoardManager.InitFromStateModel(gameType, boardStateModel);

        // Assert
        var result = scrabbleBoardManager.BoardStateModel;

        Assert.NotNull(result);
        Assert.NotNull(result.Tiles);

        Assert.Equal("A", result.Tiles[3, 3].Letter);
        Assert.Equal("B", result.Tiles[3, 4].Letter);
        Assert.Equal("C", result.Tiles[3, 5].Letter);
        Assert.Equal("D", result.Tiles[3, 6].Letter);
    }

    [Fact]
    public void MakeMoveScoring()
    {
        // Arrange
        var scrabbleBoardManager = new ScrabbleBoardManager(_mockTileFactory.Object);

        // Act
        var boardTiles = GenerateBoardTiles(GameType.StandardBoard);

        var boardStateModel = new BoardStateModel
        {
            Tiles = boardTiles
        };
        scrabbleBoardManager.InitFromStateModel(GameType.StandardBoard, boardStateModel);

        var playerMove = new PlayerMove
        {
            PlayerId = Guid.NewGuid(),
            Tiles = new List<Tile>
            {
                new()
                {
                    PosX = 0,
                    PosY = 0,
                    Letter = "Q",
                    RackPosition = 1
                },
                new()
                {
                    PosX = 3,
                    PosY = 7,
                    Letter = "E",
                    RackPosition = -1
                },
                new()
                {
                    PosX = 4,
                    PosY = 7,
                    Letter = "F",
                    RackPosition = -1
                },
                new()
                {
                    PosX = 5,
                    PosY = 7,
                    Letter = "G",
                    RackPosition = -1
                }
            }
        };

        // Assert
        var result = scrabbleBoardManager.MakeMove(playerMove);

        Assert.NotNull(result);
        Assert.Equal(playerMove.PlayerId, result.PlayerId);
        Assert.Equivalent(playerMove.Tiles, result.Tiles);

        Assert.Equal(17, result.Points);
    }

    [Fact]
    public void MakeMoveScoringSevenLetter()
    {
        // Arrange
        var scrabbleBoardManager = new ScrabbleBoardManager(_mockTileFactory.Object);

        // Act
        var boardTiles = GenerateBoardTiles(GameType.StandardBoard);

        var boardStateModel = new BoardStateModel
        {
            Tiles = boardTiles
        };
        scrabbleBoardManager.InitFromStateModel(GameType.StandardBoard, boardStateModel);

        var playerMove = new PlayerMove
        {
            PlayerId = Guid.NewGuid(),
            Tiles = new List<Tile>
            {
                new()
                {
                    PosX = 0,
                    PosY = 0,
                    Letter = "Q",
                    RackPosition = 1
                },
                new()
                {
                    PosX = 3,
                    PosY = 7,
                    Letter = "E",
                    RackPosition = -1
                },
                new()
                {
                    PosX = 4,
                    PosY = 7,
                    Letter = "F",
                    RackPosition = -1
                },
                new()
                {
                    PosX = 5,
                    PosY = 7,
                    Letter = "G",
                    RackPosition = -1
                },
                new()
                {
                    PosX = 6,
                    PosY = 7,
                    Letter = "H",
                    RackPosition = -1
                },
                new()
                {
                    PosX = 7,
                    PosY = 7,
                    Letter = "I",
                    RackPosition = -1
                },
                new()
                {
                    PosX = 8,
                    PosY = 7,
                    Letter = "G",
                    RackPosition = -1
                },
                new()
                {
                    PosX = 9,
                    PosY = 7,
                    Letter = "G",
                    RackPosition = -1
                }
            }
        };

        // Assert
        var result = scrabbleBoardManager.MakeMove(playerMove);

        Assert.NotNull(result);
        Assert.Equal(playerMove.PlayerId, result.PlayerId);
        Assert.Equivalent(playerMove.Tiles, result.Tiles);

        Assert.Equal(76, result.Points);
    }

    [Fact]
    public void MakeMoveBoardState()
    {
        // Arrange
        var scrabbleBoardManager = new ScrabbleBoardManager(_mockTileFactory.Object);

        // Act
        var boardTiles = GenerateBoardTiles(GameType.StandardBoard);

        var boardStateModel = new BoardStateModel
        {
            Tiles = boardTiles
        };
        scrabbleBoardManager.InitFromStateModel(GameType.StandardBoard, boardStateModel);

        var playerMove = new PlayerMove
        {
            PlayerId = Guid.NewGuid(),
            Tiles = new List<Tile>
            {
                new()
                {
                    PosX = 0,
                    PosY = 0,
                    Letter = "Q",
                    RackPosition = 1
                },
                new()
                {
                    PosX = 3,
                    PosY = 7,
                    Letter = "E",
                    RackPosition = -1
                },
                new()
                {
                    PosX = 4,
                    PosY = 7,
                    Letter = "F",
                    RackPosition = -1
                },
                new()
                {
                    PosX = 5,
                    PosY = 7,
                    Letter = "G",
                    RackPosition = -1
                }
            }
        };

        // Assert
        _ = scrabbleBoardManager.MakeMove(playerMove);

        var result = scrabbleBoardManager.BoardStateModel;

        Assert.Equal("E", result.Tiles[3, 7].Letter);
        Assert.Equal("F", result.Tiles[4, 7].Letter);
        Assert.Equal("G", result.Tiles[5, 7].Letter);
    }

    [Fact]
    public void UndoMove()
    {
        // Arrange
        var scrabbleBoardManager = new ScrabbleBoardManager(_mockTileFactory.Object);

        var playerTiles = new List<Tile>
        {
            new()
            {
                PosX = 0,
                PosY = 0,
                Letter = "Q",
                RackPosition = 1
            },
            new()
            {
                PosX = 3,
                PosY = 7,
                Letter = "E",
                RackPosition = -1
            },
            new()
            {
                PosX = 4,
                PosY = 7,
                Letter = "F",
                RackPosition = -1
            },
            new()
            {
                PosX = 5,
                PosY = 7,
                Letter = "G",
                RackPosition = -1
            },
            new()
            {
                PosX = 0,
                PosY = 0,
                Letter = "A",
                RackPosition = 5
            }
        };

        // Act / Assert
        var boardTiles = GenerateBoardTiles(GameType.StandardBoard);

        var boardStateModel = new BoardStateModel
        {
            Tiles = boardTiles
        };
        scrabbleBoardManager.InitFromStateModel(GameType.StandardBoard, boardStateModel);

        var playerMove = new PlayerMove
        {
            PlayerId = Guid.NewGuid(),
            Tiles = playerTiles
        };

        _ = scrabbleBoardManager.MakeMove(playerMove);

        var result = scrabbleBoardManager.BoardStateModel;

        Assert.Equal("E", result.Tiles[3, 7].Letter);
        Assert.Equal("F", result.Tiles[4, 7].Letter);
        Assert.Equal("G", result.Tiles[5, 7].Letter);

        scrabbleBoardManager.UndoMove(playerTiles);

        Assert.Equal("", result.Tiles[3, 7].Letter);
        Assert.Equal("", result.Tiles[4, 7].Letter);
        Assert.Equal("", result.Tiles[5, 7].Letter);
    }

    private static Tile[,] GenerateBoardTiles(GameType gameType)
    {
        var boardWidth = ScrabbleBoardSizes.Boards[gameType].Width;
        var boardHeight = ScrabbleBoardSizes.Boards[gameType].Height;

        var tiles = new Tile[boardWidth, boardHeight];

        for (var i = 0; i < boardWidth; i++)
        for (var j = 0; j < boardHeight; j++)
            tiles[i, j] = new Tile
            {
                TileId = Guid.NewGuid(),
                PosX = i,
                PosY = j
            };

        tiles[3, 3].Letter = "A";
        tiles[3, 4].Letter = "B";
        tiles[3, 5].Letter = "C";
        tiles[3, 6].Letter = "D";

        return tiles;
    }
}
