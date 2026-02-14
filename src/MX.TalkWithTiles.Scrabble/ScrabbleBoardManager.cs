using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    //TODO: Support different letter values based on the game type
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
        for (var j = 0; j < BoardSize.Height; j++)
        {
            var tile = _tileFactory.CreateTileForPosition(gameType, i, j);
            Tiles[i, j] = tile;
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

    //TODO: Fix this method
    private bool InvalidGaps()
    {
        var invalidGaps = false;

        for (var x = 0; x <= 14; x++)
        {
            var userTileFound = false;
            var spaceFound = false;

            for (var y = 0; y <= 14; y++)
            {
                var tile = Tiles![x, y];

                if (!tile.LetterSet && userTileFound)
                    spaceFound = true;

                if (tile.LetterSet)
                {
                    if (userTileFound && spaceFound) invalidGaps = true;
                    userTileFound = true;
                }
            }
        }

        for (var y = 0; y <= 14; y++)
        {
            var userTileFound = false;
            var spaceFound = false;
            for (var x = 0; x <= 14; x++)
            {
                var tile = Tiles![x, y];

                if (!tile.LetterSet && userTileFound)
                    spaceFound = true;

                if (tile.LetterSet)
                {
                    if (userTileFound && spaceFound) invalidGaps = true;
                    userTileFound = true;
                }
            }
        }

        return invalidGaps;
    }

    //TODO: Improve the logging of this method
    private void ScoreHorizontal(IReadOnlyCollection<Tile> tiles, ref PlayerMoveResult playerMoveResult)
    {
        for (var i = 0; i < BoardSize.Height; i++)
        {
            var word = string.Empty;
            var userHasContributedToWord = false;
            var wordScore = 0;
            var wordScoreMultiplier = 1;

            for (var j = 0; j < BoardSize.Width; j++)
            {
                var tile = Tiles![j, i];

                if (tile.LetterSet)
                {
                    word += tile.Letter;

                    var isUsersPlacedTile = tiles.Any(t => t.PosX == j && t.PosY == i);

                    if (!userHasContributedToWord && isUsersPlacedTile)
                        userHasContributedToWord = true;

                    if (isUsersPlacedTile)
                    {
                        wordScore += LetterValue(tile.Letter!) * GetLetterMultiplier(j, i);
                        wordScoreMultiplier = GetWordMultiplier(wordScoreMultiplier, j, i);
                    }
                    else
                    {
                        wordScore += LetterValue(tile.Letter!);
                    }
                }
                else
                {
                    if (userHasContributedToWord && word.Length > 1)
                    {
                        var pointsToAward = wordScore * wordScoreMultiplier;
                        playerMoveResult.Points += pointsToAward;
                        playerMoveResult.WordsAndPoints.Add(new WordAndScore
                            { Word = word, Score = pointsToAward });
                        Trace.WriteLine($"Giving {pointsToAward} for {word}");
                    }

                    word = string.Empty;
                    userHasContributedToWord = false;
                    wordScore = 0;
                    wordScoreMultiplier = 1;
                }
            }

            if (userHasContributedToWord && word.Length > 1)
            {
                var pointsToAward = wordScore * wordScoreMultiplier;
                playerMoveResult.Points += pointsToAward;
                playerMoveResult.WordsAndPoints.Add(new WordAndScore
                    { Word = word, Score = pointsToAward });
                Trace.WriteLine($"Giving {pointsToAward} for {word}");
            }
        }
    }

    //TODO: Improve the logging of this method
    private void ScoreVertical(IReadOnlyCollection<Tile> tiles, ref PlayerMoveResult playerMoveResult)
    {
        for (var i = 0; i < BoardSize.Width; i++)
        {
            var word = string.Empty;
            var userHasContributedToWord = false;
            var wordScore = 0;
            var wordScoreMultiplier = 1;

            for (var j = 0; j < BoardSize.Height; j++)
            {
                var tile = Tiles![i, j];

                if (tile.LetterSet)
                {
                    word += tile.Letter;

                    var isUsersPlacedTile = tiles.Any(t => t.PosX == i && t.PosY == j);

                    if (!userHasContributedToWord && isUsersPlacedTile)
                        userHasContributedToWord = true;

                    if (isUsersPlacedTile)
                    {
                        wordScore += LetterValue(tile.Letter!) * GetLetterMultiplier(i, j);
                        wordScoreMultiplier = GetWordMultiplier(wordScoreMultiplier, i, j);
                    }
                    else
                    {
                        wordScore += LetterValue(tile.Letter!);
                    }
                }
                else
                {
                    if (userHasContributedToWord && word.Length > 1)
                    {
                        var pointsToAward = wordScore * wordScoreMultiplier;
                        playerMoveResult.Points += pointsToAward;
                        playerMoveResult.WordsAndPoints.Add(new WordAndScore
                            { Word = word, Score = pointsToAward });
                        Trace.WriteLine($"Giving {pointsToAward} for {word}");
                    }

                    word = string.Empty;
                    userHasContributedToWord = false;
                    wordScore = 0;
                    wordScoreMultiplier = 1;
                }
            }

            if (userHasContributedToWord && word.Length > 1)
            {
                var pointsToAward = wordScore * wordScoreMultiplier;
                playerMoveResult.Points += pointsToAward;
                playerMoveResult.WordsAndPoints.Add(new WordAndScore
                    { Word = word, Score = pointsToAward });
                Trace.WriteLine($"Giving {pointsToAward} for {word}");
            }
        }
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