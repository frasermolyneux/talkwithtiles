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
    private Tile[,]? Tiles { get; set; }

    private BoardSize BoardSize { get; set; } = null!;

    public BoardStateModel BoardStateModel =>
        new()
        {
            Tiles = Tiles
        };

    public PlayerMoveResult MakeMove(PlayerMove playerMove)
    {
        var playerMoveResult = new PlayerMoveResult(playerMove.PlayerId) { Tiles = playerMove.Tiles };
        List<Tile> playerTilesOnBoard = [];

        foreach (var tile in playerMove.Tiles!.Where(t => t.RackPosition == -1))
        {
            var tileOnBoard = Tiles![tile.PosX, tile.PosY];
            tileOnBoard.Letter = tile.Letter;
            playerTilesOnBoard.Add(tileOnBoard);
        }

        ScoreHorizontal(playerTilesOnBoard, ref playerMoveResult);
        ScoreVertical(playerTilesOnBoard, ref playerMoveResult);

        if (playerTilesOnBoard.Count == 7)
            playerMoveResult.Points += 50;
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
            var tileOnBoard = Tiles![tile.PosX, tile.PosY];
            tileOnBoard.Letter = string.Empty;
        }
    }

    public void InitNew(GameType gameType)
    {
        GameType = gameType;
        BoardSize = ScrabbleBoardSizes.Boards[gameType];
        Tiles = new Tile[BoardSize.Width, BoardSize.Height];

        for (var i = 0; i < BoardSize.Width; i++)
        {
            for (var j = 0; j < BoardSize.Height; j++)
            {
                var tile = _tileFactory.CreateTileForPosition(gameType, i, j);
                Tiles[i, j] = tile;
            }
        }
    }

    public void InitFromStateModel(GameType gameType, BoardStateModel boardStateModel)
    {
        GameType = gameType;
        BoardSize = ScrabbleBoardSizes.Boards[gameType];
        Tiles = new Tile[BoardSize.Width, BoardSize.Height];

        if (boardStateModel.Tiles == null)
        {
            Tiles = null;
            return;
        }

        foreach (var tile in boardStateModel.Tiles) Tiles[tile.PosX, tile.PosY] = tile;
    }

    private void ScoreHorizontal(IReadOnlyCollection<Tile> tiles, ref PlayerMoveResult playerMoveResult)
    {
        ScoreWords(ref playerMoveResult, tiles, BoardSize.Height, BoardSize.Width,
            (i, j) => Tiles![j, i], (i, j) => (j, i));
    }

    private void ScoreVertical(IReadOnlyCollection<Tile> tiles, ref PlayerMoveResult playerMoveResult)
    {
        ScoreWords(ref playerMoveResult, tiles, BoardSize.Width, BoardSize.Height,
            (i, j) => Tiles![i, j], (i, j) => (i, j));
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
            wordScore += LetterValue(tile.Letter!) * GetLetterMultiplier(x, y);
            wordScoreMultiplier = GetWordMultiplier(wordScoreMultiplier, x, y);
        }
        else
        {
            wordScore += LetterValue(tile.Letter!);
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
        var code = Tiles![x, y];
        if (code.TileType == TileType.TripleWordScoreTile)
            return currentMultiplier * 3;
        if (code.TileType == TileType.DoubleWordScoreTile ||
            code.TileType == TileType.CentreTile)
            return currentMultiplier * 2;

        return currentMultiplier;
    }

    private int GetLetterMultiplier(int x, int y)
    {
        var code = Tiles![x, y];
        if (code.TileType == TileType.DoubleLetterScoreTile)
            return 2;
        if (code.TileType == TileType.TripleLetterScoreTile)
            return 3;

        return 1;
    }
}