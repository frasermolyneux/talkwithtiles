using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.Scrabble.Constants;
using Xunit;

namespace MX.TalkWithTiles.Scrabble.Tests;

public class ScrabbleBoardManagerValidationTests
{
    private readonly ScrabbleBoardManager _boardManager;

    public ScrabbleBoardManagerValidationTests()
    {
        var mockTileFactory = new Mock<ITileFactory>();
        _boardManager = new ScrabbleBoardManager(mockTileFactory.Object);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates an empty 15×15 StandardBoard with CentreTile at (7,7).
    /// No letters are placed — simulates the very first move.
    /// </summary>
    private void InitEmptyBoard()
    {
        var boardSize = ScrabbleBoardSizes.Boards[GameType.StandardBoard];
        var tiles = new Tile[boardSize.Width, boardSize.Height];

        for (var x = 0; x < boardSize.Width; x++)
        for (var y = 0; y < boardSize.Height; y++)
            tiles[x, y] = new Tile
            {
                TileId = Guid.NewGuid(),
                PosX = x,
                PosY = y,
                TileType = (x == 7 && y == 7) ? TileType.CentreTile : TileType.StandardTile
            };

        _boardManager.InitFromStateModel(GameType.StandardBoard, new BoardStateModel { Tiles = tiles });
    }

    /// <summary>
    /// Creates a StandardBoard with letters already placed at (7,7)="A" and (8,7)="B".
    /// Simulates a board after the first move — used for connectivity tests.
    /// </summary>
    private void InitBoardWithExistingTiles()
    {
        var boardSize = ScrabbleBoardSizes.Boards[GameType.StandardBoard];
        var tiles = new Tile[boardSize.Width, boardSize.Height];

        for (var x = 0; x < boardSize.Width; x++)
        for (var y = 0; y < boardSize.Height; y++)
            tiles[x, y] = new Tile
            {
                TileId = Guid.NewGuid(),
                PosX = x,
                PosY = y,
                TileType = (x == 7 && y == 7) ? TileType.CentreTile : TileType.StandardTile
            };

        tiles[7, 7].Letter = "A";
        tiles[8, 7].Letter = "B";

        _boardManager.InitFromStateModel(GameType.StandardBoard, new BoardStateModel { Tiles = tiles });
    }

    private static PlayerMove CreateMove(params (int x, int y, string letter)[] placements)
    {
        var tiles = new List<Tile>();
        foreach (var (x, y, letter) in placements)
            tiles.Add(new Tile { PosX = x, PosY = y, Letter = letter, RackPosition = -1 });

        return new PlayerMove { PlayerId = Guid.NewGuid(), Tiles = tiles };
    }

    // -----------------------------------------------------------------------
    // Bounds check
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(-1, 7, "X coordinate below zero")]
    [InlineData(7, -1, "Y coordinate below zero")]
    [InlineData(15, 7, "X coordinate at board width")]
    [InlineData(7, 15, "Y coordinate at board height")]
    [InlineData(99, 99, "Both coordinates far out of bounds")]
    public void MakeMove_RejectsOutOfBoundsTiles(int x, int y, string scenario)
    {
        _ = scenario;
        InitEmptyBoard();
        var move = CreateMove((x, y, "A"));

        var result = _boardManager.MakeMove(move);

        Assert.False(result.IsValid);
        Assert.Contains("bounds", result.InvalidMessage!, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------------------------------------------------
    // Occupancy check
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_RejectsPlacementOnOccupiedCell()
    {
        InitBoardWithExistingTiles();
        var move = CreateMove((7, 7, "Z")); // (7,7) already has "A"

        var result = _boardManager.MakeMove(move);

        Assert.False(result.IsValid);
        Assert.Matches("occupied|already", result.InvalidMessage!.ToLowerInvariant());
    }

    [Fact]
    public void MakeMove_AcceptsPlacementOnEmptyAdjacentCell()
    {
        InitBoardWithExistingTiles();
        var move = CreateMove((9, 7, "C")); // empty, adjacent to "B" at (8,7)

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    // -----------------------------------------------------------------------
    // Linear placement check
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_RejectsDiagonalPlacement()
    {
        InitEmptyBoard();
        var move = CreateMove((7, 7, "A"), (8, 8, "B")); // diagonal

        var result = _boardManager.MakeMove(move);

        Assert.False(result.IsValid);
        Assert.Matches("line|row|column|straight", result.InvalidMessage!.ToLowerInvariant());
    }

    [Fact]
    public void MakeMove_RejectsLShapedPlacement()
    {
        InitEmptyBoard();
        // L-shape: two in a row, then one turning the corner
        var move = CreateMove((7, 7, "A"), (8, 7, "B"), (8, 8, "C"));

        var result = _boardManager.MakeMove(move);

        Assert.False(result.IsValid);
        Assert.Matches("line|row|column|straight", result.InvalidMessage!.ToLowerInvariant());
    }

    [Fact]
    public void MakeMove_AcceptsHorizontalLine()
    {
        InitEmptyBoard();
        var move = CreateMove((6, 7, "A"), (7, 7, "B"), (8, 7, "C")); // same row

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void MakeMove_AcceptsVerticalLine()
    {
        InitEmptyBoard();
        var move = CreateMove((7, 6, "A"), (7, 7, "B"), (7, 8, "C")); // same column

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void MakeMove_SingleTileAlwaysPassesLinearCheck()
    {
        InitEmptyBoard();
        var move = CreateMove((7, 7, "A"));

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    // -----------------------------------------------------------------------
    // Center star check (first move on empty board)
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_RejectsFirstMoveNotOnCenter()
    {
        InitEmptyBoard();
        var move = CreateMove((0, 0, "A"));

        var result = _boardManager.MakeMove(move);

        Assert.False(result.IsValid);
        Assert.Contains("center", result.InvalidMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MakeMove_RejectsFirstMoveAdjacentToButNotCrossingCenter()
    {
        InitEmptyBoard();
        // Tiles at (6,7) and (8,7) — straddle center but skip it
        var move = CreateMove((6, 7, "A"), (8, 7, "B"));

        var result = _boardManager.MakeMove(move);

        Assert.False(result.IsValid);
        Assert.Contains("center", result.InvalidMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MakeMove_AcceptsFirstMoveCrossingCenter()
    {
        InitEmptyBoard();
        var move = CreateMove((7, 7, "A"), (8, 7, "B"));

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void MakeMove_AcceptsSingleTileOnCenter()
    {
        InitEmptyBoard();
        var move = CreateMove((7, 7, "A"));

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    // -----------------------------------------------------------------------
    // Connectivity check (subsequent moves on non-empty board)
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_RejectsDisconnectedSecondMove()
    {
        InitBoardWithExistingTiles(); // (7,7)="A", (8,7)="B"
        var move = CreateMove((0, 0, "Z")); // far from any existing tile

        var result = _boardManager.MakeMove(move);

        Assert.False(result.IsValid);
        Assert.Matches("connect|adjacent|attach", result.InvalidMessage!.ToLowerInvariant());
    }

    [Fact]
    public void MakeMove_RejectsMoveTwoSquaresAway()
    {
        InitBoardWithExistingTiles(); // (7,7)="A", (8,7)="B"
        var move = CreateMove((10, 7, "Z")); // same row but 2 cells away from "B"

        var result = _boardManager.MakeMove(move);

        Assert.False(result.IsValid);
        Assert.Matches("connect|adjacent|attach", result.InvalidMessage!.ToLowerInvariant());
    }

    [Fact]
    public void MakeMove_AcceptsAdjacentBelow()
    {
        InitBoardWithExistingTiles(); // (7,7)="A", (8,7)="B"
        var move = CreateMove((7, 8, "C")); // below (7,7)

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void MakeMove_AcceptsAdjacentAbove()
    {
        InitBoardWithExistingTiles(); // (7,7)="A", (8,7)="B"
        var move = CreateMove((7, 6, "C")); // above (7,7)

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void MakeMove_AcceptsAdjacentLeft()
    {
        InitBoardWithExistingTiles(); // (7,7)="A", (8,7)="B"
        var move = CreateMove((6, 7, "C")); // left of (7,7)

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void MakeMove_AcceptsAdjacentRight()
    {
        InitBoardWithExistingTiles(); // (7,7)="A", (8,7)="B"
        var move = CreateMove((9, 7, "C")); // right of (8,7)

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void MakeMove_ConnectivityOnlyNeedsOneTileAdjacent()
    {
        InitBoardWithExistingTiles(); // (7,7)="A", (8,7)="B"
        // Vertical word: (9,6), (9,7) — only (9,7) is adjacent to (8,7)
        var move = CreateMove((9, 6, "C"), (9, 7, "D"));

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
    }

    // -----------------------------------------------------------------------
    // No tiles placed (all remain on rack)
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_ReturnsValidWhenNoTilesPlaced()
    {
        InitEmptyBoard();
        var move = new PlayerMove
        {
            PlayerId = Guid.NewGuid(),
            Tiles =
            [
                new Tile { PosX = 0, PosY = 0, Letter = "A", RackPosition = 0 }
            ]
        };

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
        Assert.Equal(0, result.Points);
    }

    // -----------------------------------------------------------------------
    // Scoring still works after validation passes
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_ValidMoveStillCalculatesScore()
    {
        InitBoardWithExistingTiles(); // (7,7)="A", (8,7)="B"
        var move = CreateMove((9, 7, "C")); // extends the word to "ABC"

        var result = _boardManager.MakeMove(move);

        Assert.True(result.IsValid);
        Assert.True(result.Points > 0);
        Assert.NotEmpty(result.WordsAndPoints);
    }

    [Fact]
    public void MakeMove_InvalidMoveReturnsZeroPoints()
    {
        InitBoardWithExistingTiles();
        var move = CreateMove((0, 0, "Z")); // disconnected

        var result = _boardManager.MakeMove(move);

        Assert.False(result.IsValid);
        Assert.Equal(0, result.Points);
        Assert.Empty(result.WordsAndPoints);
    }
}
