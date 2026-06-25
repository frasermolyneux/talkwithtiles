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

public class ScrabbleBoardManagerScoringTests
{
    private readonly ScrabbleBoardManager _boardManager;

    public ScrabbleBoardManagerScoringTests()
    {
        var mockTileFactory = new Mock<ITileFactory>();
        _boardManager = new ScrabbleBoardManager(mockTileFactory.Object);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates a 15×15 StandardBoard with all premium squares correctly applied.
    /// Optionally places existing tiles for connectivity tests.
    /// </summary>
    private void InitBoardWithPremiumSquares(params (int x, int y, string letter)[] existingTiles)
    {
        var boardSize = ScrabbleBoardSizes.Boards[GameType.StandardBoard];
        var tiles = new Tile[boardSize.Width, boardSize.Height];

        for (var x = 0; x < boardSize.Width; x++)
        {
            for (var y = 0; y < boardSize.Height; y++)
            {
                tiles[x, y] = new Tile
                {
                    TileId = Guid.NewGuid(),
                    PosX = x,
                    PosY = y,
                    TileType = TileType.StandardTile
                };
            }
        }

        // Apply premium squares from the actual board constants
        foreach (var premiumTile in ScrabbleBoardTiles.Tiles[GameType.StandardBoard])
        {
            tiles[premiumTile.PosX, premiumTile.PosY].TileType = premiumTile.TileType;
        }

        // Place any existing tiles
        foreach (var (x, y, letter) in existingTiles)
        {
            tiles[x, y].Letter = letter;
        }

        _boardManager.InitFromStateModel(GameType.StandardBoard, new BoardStateModel { Tiles = tiles });
    }

    private static PlayerMove CreateMove(params (int x, int y, string letter)[] placements)
    {
        var tiles = new List<Tile>();
        foreach (var (x, y, letter) in placements)
        {
            tiles.Add(new Tile { PosX = x, PosY = y, Letter = letter, RackPosition = -1 });
        }

        return new PlayerMove { PlayerId = Guid.NewGuid(), Tiles = tiles };
    }

    // -----------------------------------------------------------------------
    // Center Star — 2× Word Multiplier
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_CenterStarApplies2xWordMultiplier()
    {
        // Arrange — empty board with premium squares
        InitBoardWithPremiumSquares();

        // Place "E"(1pt) at center (7,7) and "A"(1pt) at (8,7)
        // Center = CentreTile → 2× word multiplier
        // Expected: (1 + 1) * 2 = 4
        var move = CreateMove((7, 7, "E"), (8, 7, "A"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(4, result.Points);
        Assert.Single(result.WordsAndPoints);
        Assert.Equal(4, result.WordsAndPoints[0].Score);
    }

    [Fact]
    public void MakeMove_CenterStarMultipliesEntireWord()
    {
        // Arrange — place higher-value tiles through center
        InitBoardWithPremiumSquares();

        // Place "Z"(10pt) at (7,7) and "A"(1pt) at (8,7)
        // Expected: (10 + 1) * 2 = 22
        var move = CreateMove((7, 7, "Z"), (8, 7, "A"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(22, result.Points);
    }

    // -----------------------------------------------------------------------
    // Double Letter Score
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_DoubleLetterScoreDoublesTileValue()
    {
        // Arrange — existing tiles at (7,7)="A" and (8,7)="B"
        // (8,6) is DoubleLetterScoreTile on StandardBoard
        InitBoardWithPremiumSquares((7, 7, "A"), (8, 7, "B"));

        // Place "E"(1pt) at (8,6) — DL position, adjacent to (8,7)
        // Vertical word at x=8: E(1×2) + B(3) = 5
        var move = CreateMove((8, 6, "E"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(5, result.Points);
    }

    [Fact]
    public void MakeMove_DoubleLetterOnHighValueTile()
    {
        // Arrange — existing tile at (8,7)="A"
        // (8,6) is DoubleLetterScoreTile
        InitBoardWithPremiumSquares((8, 7, "A"));

        // Place "Q"(10pt) at (8,6) — DL → 10×2 = 20, + A(1) = 21
        var move = CreateMove((8, 6, "Q"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(21, result.Points);
    }

    // -----------------------------------------------------------------------
    // Triple Letter Score
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_TripleLetterScoreTriplesTileValue()
    {
        // Arrange — existing tile at (5,6)="A"
        // (5,5) is TripleLetterScoreTile on StandardBoard
        InitBoardWithPremiumSquares((5, 6, "A"));

        // Place "E"(1pt) at (5,5) — TL → 1×3 = 3, + A(1) = 4
        var move = CreateMove((5, 5, "E"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(4, result.Points);
    }

    [Fact]
    public void MakeMove_TripleLetterOnHighValueTile()
    {
        // Arrange — existing tile at (5,6)="A"
        // (5,5) is TripleLetterScoreTile
        InitBoardWithPremiumSquares((5, 6, "A"));

        // Place "J"(8pt) at (5,5) — TL → 8×3 = 24, + A(1) = 25
        var move = CreateMove((5, 5, "J"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(25, result.Points);
    }

    // -----------------------------------------------------------------------
    // Double Word Score
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_DoubleWordScoreDoublesEntireWord()
    {
        // Arrange — existing tile at (4,5)="A"
        // (4,4) is DoubleWordScoreTile on StandardBoard
        InitBoardWithPremiumSquares((4, 5, "A"));

        // Place "E"(1pt) at (4,4) — DW position
        // Vertical at x=4: E(1) + A(1) = 2, × 2 = 4
        var move = CreateMove((4, 4, "E"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(4, result.Points);
    }

    [Fact]
    public void MakeMove_DoubleWordScoreMultipliesFullWordNotJustTile()
    {
        // Arrange — existing tiles to form a longer word
        InitBoardWithPremiumSquares((4, 5, "A"), (4, 6, "B"));

        // Place "E"(1pt) at (4,4) — DW position
        // Vertical at x=4: E(1) + A(1) + B(3) = 5, × 2 = 10
        var move = CreateMove((4, 4, "E"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(10, result.Points);
    }

    // -----------------------------------------------------------------------
    // Triple Word Score
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_TripleWordScoreTriplesEntireWord()
    {
        // Arrange — existing tile at (1,0)="A"
        // (0,0) is TripleWordScoreTile on StandardBoard
        InitBoardWithPremiumSquares((1, 0, "A"));

        // Place "E"(1pt) at (0,0) — TW position
        // Horizontal at y=0: E(1) + A(1) = 2, × 3 = 6
        var move = CreateMove((0, 0, "E"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(6, result.Points);
    }

    [Fact]
    public void MakeMove_TripleWordScoreMultipliesFullWord()
    {
        // Arrange — existing tiles
        InitBoardWithPremiumSquares((1, 0, "A"), (2, 0, "B"));

        // Place "E"(1pt) at (0,0) — TW position
        // Horizontal at y=0: E(1) + A(1) + B(3) = 5, × 3 = 15
        var move = CreateMove((0, 0, "E"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(15, result.Points);
    }

    // -----------------------------------------------------------------------
    // Cross-Word Scoring — multiple words counted
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_CrossWordScoringCountsBothWords()
    {
        // Arrange — existing tiles at (7,7)="A" and (8,7)="B" (horizontal)
        // Place "C" at (9,7) and "D" at (9,8) (vertical extending right)
        // This forms:
        //   Horizontal y=7: extends ABC (but only C is new, so scores "ABC")
        //   Vertical x=9: new word "CD"
        InitBoardWithPremiumSquares((7, 7, "A"), (8, 7, "B"));

        var move = CreateMove((9, 7, "C"), (9, 8, "D"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(2, result.WordsAndPoints.Count);

        // Horizontal: A(1) + B(3) + C(3) = 7 (no multipliers at 9,7)
        var horizontalWord = result.WordsAndPoints.First(w => w.Word == "ABC");
        Assert.Equal(7, horizontalWord.Score);

        // Vertical: C(3) + D(2) = 5 (no multipliers at 9,7 or 9,8)
        var verticalWord = result.WordsAndPoints.First(w => w.Word == "CD");
        Assert.Equal(5, verticalWord.Score);

        Assert.Equal(12, result.Points);
    }

    [Fact]
    public void MakeMove_CrossWordScoringWithMultiplier()
    {
        // Arrange — existing tiles at (7,7)="A" and (8,7)="B"
        // Place "C" at (6,7) and "D" at (6,8)
        // (6,8) is DoubleLetterScoreTile on StandardBoard
        InitBoardWithPremiumSquares((7, 7, "A"), (8, 7, "B"));

        var move = CreateMove((6, 7, "C"), (6, 8, "D"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(2, result.WordsAndPoints.Count);

        // Horizontal y=7: C(3) + A(1) + B(3) = 7
        var horizontalWord = result.WordsAndPoints.First(w => w.Word.Contains("A"));
        Assert.Equal(7, horizontalWord.Score);

        // Vertical x=6: C(3) + D(2×2=4) = 7 (D on DL at 6,8)
        var verticalWord = result.WordsAndPoints.First(w => w.Word == "CD");
        Assert.Equal(7, verticalWord.Score);

        Assert.Equal(14, result.Points);
    }

    // -----------------------------------------------------------------------
    // 50-Point Bonus for 7 Tiles
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_SevenTilesAdds50PointBonus()
    {
        // Arrange — empty board with premium squares
        // Place 7 "E"(1pt) tiles horizontally at (4,7)-(10,7), crossing center at (7,7)
        InitBoardWithPremiumSquares();

        var move = CreateMove(
            (4, 7, "E"), (5, 7, "E"), (6, 7, "E"), (7, 7, "E"),
            (8, 7, "E"), (9, 7, "E"), (10, 7, "E"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);

        // Word score: 7 × E(1) = 7, center (7,7) × 2 = 14
        var wordScore = result.WordsAndPoints.Sum(w => w.Score);
        Assert.Equal(14, wordScore);

        // Total includes +50 bonus
        Assert.Equal(64, result.Points);
        Assert.Equal(wordScore + 50, result.Points);
    }

    [Fact]
    public void MakeMove_SixTilesDoesNotGetBonus()
    {
        // Arrange — empty board, place only 6 tiles
        InitBoardWithPremiumSquares();

        var move = CreateMove(
            (5, 7, "E"), (6, 7, "E"), (7, 7, "E"),
            (8, 7, "E"), (9, 7, "E"), (10, 7, "E"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert
        Assert.True(result.IsValid);

        // Word score: 6 × E(1) = 6, center (7,7) × 2 = 12
        var wordScore = result.WordsAndPoints.Sum(w => w.Score);
        Assert.Equal(12, wordScore);

        // No bonus — total equals word score exactly
        Assert.Equal(wordScore, result.Points);
    }

    // -----------------------------------------------------------------------
    // Premium squares only apply to placed tiles, not existing ones
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_PremiumSquaresOnlyApplyToNewlyPlacedTiles()
    {
        // Arrange — existing tile at (0,0) which is TW.
        // Place "A"(1pt) at (0,1) — standard position, adjacent to existing
        // The TW at (0,0) should NOT multiply the word again
        InitBoardWithPremiumSquares((0, 0, "Z"));

        var move = CreateMove((0, 1, "A"));

        // Act
        var result = _boardManager.MakeMove(move);

        // Assert — vertical at x=0: Z(10, existing no mult) + A(1, placed, no mult) = 11, wordMult=1
        Assert.True(result.IsValid);
        Assert.Equal(11, result.Points);
    }
}
