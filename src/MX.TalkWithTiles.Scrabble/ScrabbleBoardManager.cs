using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.Scrabble.Constants;
using MX.TalkWithTiles.Scrabble.Helpers;

namespace MX.TalkWithTiles.Scrabble;

public class ScrabbleBoardManager(ITileFactory tileFactory) : IBoardManager
{
    private readonly ITileFactory _tileFactory = tileFactory ?? throw new ArgumentNullException(nameof(tileFactory));

    private GameType GameType { get; set; }
    private Tile[,]? _tiles;
    private Tile[,] Tiles => _tiles ?? throw new InvalidOperationException("Board not initialized. Call InitNew or InitFromStateModel first.");
    private BoardSize? _boardSize;
    private BoardSize BoardSize
    {
        get => _boardSize ?? throw new InvalidOperationException("Board not initialized. Call InitNew or InitFromStateModel first.");
        set => _boardSize = value;
    }

    public BoardStateModel BoardStateModel =>
        new()
        {
            Tiles = _tiles
        };

    public PlayerMoveResult MakeMove(PlayerMove playerMove)
    {
        var playerMoveResult = new PlayerMoveResult(playerMove.PlayerId) { Tiles = playerMove.Tiles };
        var placedTiles = playerMove.Tiles.Where(t => t.RackPosition == -1).ToList();

        if (placedTiles.Count == 0)
        {
            return playerMoveResult;
        }

        // Bounds check
        foreach (var tile in placedTiles)
        {
            if (tile.PosX < 0 || tile.PosX >= BoardSize.Width || tile.PosY < 0 || tile.PosY >= BoardSize.Height)
            {
                playerMoveResult.InvalidMessage = "Tile placement is out of bounds.";
                return playerMoveResult;
            }
        }

        // Occupancy check — cannot place on an already occupied cell
        foreach (var tile in placedTiles)
        {
            if (Tiles[tile.PosX, tile.PosY].LetterSet)
            {
                playerMoveResult.InvalidMessage = "Cannot place a tile on an already occupied cell.";
                return playerMoveResult;
            }
        }

        // Linear check — all placed tiles must share the same row or column
        if (placedTiles.Count > 1)
        {
            var allSameRow = placedTiles.All(t => t.PosY == placedTiles[0].PosY);
            var allSameCol = placedTiles.All(t => t.PosX == placedTiles[0].PosX);
            if (!allSameRow && !allSameCol)
            {
                playerMoveResult.InvalidMessage = "Tiles must be placed in a straight line (same row or column).";
                return playerMoveResult;
            }
        }

        // First-move vs connectivity checks
        var boardIsEmpty = !HasExistingTiles();
        if (boardIsEmpty)
        {
            // Center star check — first word must cross the center
            if (!placedTiles.Any(t => Tiles[t.PosX, t.PosY].TileType == TileType.CentreTile))
            {
                playerMoveResult.InvalidMessage = "The first word must cross the center star.";
                return playerMoveResult;
            }
        }
        else
        {
            // Connectivity check — at least one placed tile must be adjacent to an existing tile
            if (!placedTiles.Any(t => HasAdjacentExistingTile(t.PosX, t.PosY)))
            {
                playerMoveResult.InvalidMessage = "Tiles must connect to or be adjacent to existing tiles on the board.";
                return playerMoveResult;
            }
        }

        // All validation passed — place tiles and calculate score
        List<Tile> playerTilesOnBoard = [];

        foreach (var tile in placedTiles)
        {
            var tileOnBoard = Tiles[tile.PosX, tile.PosY];
            tileOnBoard.Letter = tile.Letter;
            playerTilesOnBoard.Add(tileOnBoard);
        }

        ScoreHorizontal(playerTilesOnBoard, ref playerMoveResult);
        ScoreVertical(playerTilesOnBoard, ref playerMoveResult);

        if (playerTilesOnBoard.Count == 7)
        {
            playerMoveResult.Points += 50;
        }

        return playerMoveResult;
    }

    public int LetterValue(string letter)
    {
        return ScrabbleTileScoreHelper.GetTileScore(letter);
    }

    public void UndoMove(List<Tile> tiles)
    {
        foreach (var tile in tiles.Where(t => t.RackPosition == -1))
        {
            var tileOnBoard = Tiles[tile.PosX, tile.PosY];
            tileOnBoard.Letter = string.Empty;
        }
    }

    public void InitNew(GameType gameType)
    {
        GameType = gameType;
        BoardSize = ScrabbleBoardSizes.Boards[gameType];
        _tiles = new Tile[BoardSize.Width, BoardSize.Height];

        for (var i = 0; i < BoardSize.Width; i++)
        {
            for (var j = 0; j < BoardSize.Height; j++)
            {
                var tile = _tileFactory.CreateTileForPosition(gameType, i, j);
                _tiles[i, j] = tile;
            }
        }
    }

    public void InitFromStateModel(GameType gameType, BoardStateModel boardStateModel)
    {
        GameType = gameType;
        BoardSize = ScrabbleBoardSizes.Boards[gameType];

        if (boardStateModel.Tiles == null)
        {
            throw new ArgumentException("Cannot initialize board from state model with null tiles.", nameof(boardStateModel));
        }

        _tiles = new Tile[BoardSize.Width, BoardSize.Height];
        foreach (var tile in boardStateModel.Tiles)
        {
            _tiles[tile.PosX, tile.PosY] = tile;
        }
    }

    private void ScoreHorizontal(IReadOnlyCollection<Tile> tiles, ref PlayerMoveResult playerMoveResult)
    {
        ScoreWords(ref playerMoveResult, tiles, BoardSize.Height, BoardSize.Width,
            (i, j) => Tiles[j, i], (i, j) => (j, i));
    }

    private void ScoreVertical(IReadOnlyCollection<Tile> tiles, ref PlayerMoveResult playerMoveResult)
    {
        ScoreWords(ref playerMoveResult, tiles, BoardSize.Width, BoardSize.Height,
            (i, j) => Tiles[i, j], (i, j) => (i, j));
    }

    private void ScoreWords(ref PlayerMoveResult playerMoveResult, IReadOnlyCollection<Tile> tiles,
        int outerMax, int innerMax, Func<int, int, Tile> getTile, Func<int, int, (int x, int y)> getPosition)
    {
        for (var i = 0; i < outerMax; i++)
        {
            ScoreWordsInLine(ref playerMoveResult, tiles, innerMax, j => getTile(i, j), j => getPosition(i, j));
        }
    }

    private void ScoreWordsInLine(ref PlayerMoveResult playerMoveResult, IReadOnlyCollection<Tile> tiles,
        int lineLength, Func<int, Tile> getTile, Func<int, (int x, int y)> getPosition)
    {
        var wordBuilder = new StringBuilder();
        var userHasContributedToWord = false;
        var wordScore = 0;
        var wordScoreMultiplier = 1;

        for (var j = 0; j < lineLength; j++)
        {
            var tile = getTile(j);
            var (x, y) = getPosition(j);

            if (tile.LetterSet)
            {
                AccumulateTileScore(tiles, tile, x, y, wordBuilder, ref userHasContributedToWord, ref wordScore, ref wordScoreMultiplier);
            }
            else
            {
                TryFinalizeWord(ref playerMoveResult, wordBuilder, ref userHasContributedToWord, ref wordScore, ref wordScoreMultiplier);
            }
        }

        TryFinalizeWord(ref playerMoveResult, wordBuilder, ref userHasContributedToWord, ref wordScore, ref wordScoreMultiplier);
    }

    private void AccumulateTileScore(IReadOnlyCollection<Tile> tiles, Tile tile, int x, int y,
        StringBuilder wordBuilder, ref bool userHasContributedToWord, ref int wordScore, ref int wordScoreMultiplier)
    {
        wordBuilder.Append(tile.Letter);
        var isUsersPlacedTile = tiles.Any(t => t.PosX == x && t.PosY == y);

        if (isUsersPlacedTile)
        {
            userHasContributedToWord = true;
            wordScore += LetterValue(tile.Letter ?? string.Empty) * GetLetterMultiplier(x, y);
            wordScoreMultiplier = GetWordMultiplier(wordScoreMultiplier, x, y);
        }
        else
        {
            wordScore += LetterValue(tile.Letter ?? string.Empty);
        }
    }

    private static void TryFinalizeWord(ref PlayerMoveResult playerMoveResult, StringBuilder wordBuilder,
        ref bool userHasContributedToWord, ref int wordScore, ref int wordScoreMultiplier)
    {
        if (userHasContributedToWord && wordBuilder.Length > 1)
        {
            var pointsToAward = wordScore * wordScoreMultiplier;
            playerMoveResult.Points += pointsToAward;
            playerMoveResult.WordsAndPoints.Add(new WordAndScore { Word = wordBuilder.ToString(), Score = pointsToAward });
        }

        wordBuilder.Clear();
        userHasContributedToWord = false;
        wordScore = 0;
        wordScoreMultiplier = 1;
    }

    private int GetWordMultiplier(int currentMultiplier, int x, int y)
    {
        var code = Tiles[x, y];
        if (code.TileType == TileType.TripleWordScoreTile)
        {
            return currentMultiplier * 3;
        }

        if (code.TileType == TileType.DoubleWordScoreTile ||
            code.TileType == TileType.CentreTile)
        {
            return currentMultiplier * 2;
        }

        return currentMultiplier;
    }

    private int GetLetterMultiplier(int x, int y)
    {
        var code = Tiles[x, y];
        if (code.TileType == TileType.DoubleLetterScoreTile)
        {
            return 2;
        }

        if (code.TileType == TileType.TripleLetterScoreTile)
        {
            return 3;
        }

        return 1;
    }

    private bool HasExistingTiles()
    {
        for (var x = 0; x < BoardSize.Width; x++)
        {
            for (var y = 0; y < BoardSize.Height; y++)
            {
                if (Tiles[x, y].LetterSet)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasAdjacentExistingTile(int x, int y)
    {
        if (x > 0 && Tiles[x - 1, y].LetterSet)
        {
            return true;
        }

        if (x < BoardSize.Width - 1 && Tiles[x + 1, y].LetterSet)
        {
            return true;
        }

        if (y > 0 && Tiles[x, y - 1].LetterSet)
        {
            return true;
        }

        if (y < BoardSize.Height - 1 && Tiles[x, y + 1].LetterSet)
        {
            return true;
        }

        return false;
    }
}
