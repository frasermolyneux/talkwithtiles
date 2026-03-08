using System;
using System.Collections.Generic;
using Moq;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.CoreEngine.Game;
using Xunit;

namespace MX.TalkWithTiles.CoreEngine.Tests.Game;

/// <summary>
/// Tests that consecutive passes by all players trigger game end.
/// Per Scrabble rules, if every player passes in a complete round
/// (consecutive passes >= player count), the game should end.
/// 
/// BUG: SkipTurn currently does NOT track passes or trigger game end.
/// These tests define the expected behaviour and should FAIL until fixed.
/// </summary>
public class PlayerMoveManagerConsecutivePassTests
{
    private readonly Guid _playerOneId = Guid.NewGuid();
    private readonly Guid _playerTwoId = Guid.NewGuid();
    private readonly Guid _playerThreeId = Guid.NewGuid();

    private readonly Mock<IBagManager> _mockBagManager;
    private readonly Mock<IBoardManager> _mockBoardManager;
    private readonly Mock<IEndGameManager> _mockEndGameManager;
    private readonly Mock<IPlayerManager> _mockPlayerManager;

    public PlayerMoveManagerConsecutivePassTests()
    {
        _mockBagManager = new Mock<IBagManager>();
        _mockBoardManager = new Mock<IBoardManager>();
        _mockEndGameManager = new Mock<IEndGameManager>();
        _mockPlayerManager = new Mock<IPlayerManager>();
    }

    // -----------------------------------------------------------------------
    // Two-player consecutive passes should end game
    // -----------------------------------------------------------------------

    [Fact]
    public void SkipTurn_AllPlayersPassConsecutively_EndsGame()
    {
        // Arrange
        var manager = CreateManager(TwoPlayerState());
        SetupPlayersForEndGame(_playerOneId, _playerTwoId);

        // Act — both players skip (2 consecutive passes = player count)
        manager.SkipTurn(_playerOneId);  // player 1 passes, turn moves to player 2
        manager.SkipTurn(_playerTwoId);  // player 2 passes, all players have passed

        // Assert — game should end
        _mockEndGameManager.Verify(
            x => x.SetWinners(It.IsAny<List<Guid>>(), It.IsAny<int>()),
            Times.Once,
            "Game should end when all players pass consecutively");
    }

    [Fact]
    public void SkipTurn_OnePlayerPasses_DoesNotEndGame()
    {
        // Arrange
        var manager = CreateManager(TwoPlayerState());

        // Act — only one player skips
        manager.SkipTurn(_playerOneId);

        // Assert — game should NOT end yet
        _mockEndGameManager.Verify(
            x => x.SetWinners(It.IsAny<List<Guid>>(), It.IsAny<int>()),
            Times.Never,
            "Game should not end when only one player has passed");
    }

    // -----------------------------------------------------------------------
    // Normal move resets consecutive pass counter
    // -----------------------------------------------------------------------

    [Fact]
    public void MakeMove_ResetsConsecutivePassCounter()
    {
        // Arrange
        var manager = CreateManager(TwoPlayerState());
        var playerTwo = SetupPlayerForMove(_playerTwoId);

        playerTwo.Tiles[0].RackPosition = -1;
        var playerMove = new PlayerMove
        {
            PlayerId = _playerTwoId,
            Tiles = [playerTwo.Tiles[0]]
        };

        var moveResult = new PlayerMoveResult(_playerTwoId);
        _mockBoardManager.Setup(x => x.MakeMove(playerMove)).Returns(moveResult);
        _mockBagManager.Setup(x => x.TakeTiles(It.IsAny<int>())).Returns(new List<Tile>
        {
            new() { TileId = Guid.NewGuid(), Letter = "D" },
            new() { TileId = Guid.NewGuid(), Letter = "E" }
        });

        // Act — player 1 skips (current → player 2), then player 2 makes a valid move (resets counter)
        manager.SkipTurn(_playerOneId);  // 1 consecutive pass, current → player 2

        // Player 2 makes a valid move — resets consecutive passes
        manager.MakeMove(playerMove, false);

        // Both players now skip again — but counter was reset, so only 2 new passes
        manager.SkipTurn(_playerTwoId); // 1 pass (after reset)

        // Assert — game should NOT end (only 1 pass since reset, need 2)
        _mockEndGameManager.Verify(
            x => x.SetWinners(It.IsAny<List<Guid>>(), It.IsAny<int>()),
            Times.Never,
            "Consecutive pass counter should reset after a valid move");
    }

    // -----------------------------------------------------------------------
    // Exchange resets consecutive pass counter
    // -----------------------------------------------------------------------

    [Fact]
    public void ExchangeTiles_ResetsConsecutivePassCounter()
    {
        // Arrange
        var manager = CreateManager(TwoPlayerState());
        var playerTwo = SetupPlayerForMove(_playerTwoId);

        _mockBagManager.Setup(x => x.TakeTiles(It.IsAny<int>())).Returns(new List<Tile>
        {
            new() { TileId = Guid.NewGuid(), Letter = "X" }
        });

        // Act — player 1 skips (current → player 2), player 2 exchanges (resets counter)
        manager.SkipTurn(_playerOneId);  // 1 consecutive pass, current → player 2

        // Player 2 exchanges tiles — resets the counter
        var tileToExchange = playerTwo.Tiles[0].TileId;
        manager.ExchangeTiles(_playerTwoId, new[] { tileToExchange });

        // Player 1 skips — only 1 pass since reset
        manager.SkipTurn(_playerOneId);

        // Assert — game should NOT end (only 1 pass since reset)
        _mockEndGameManager.Verify(
            x => x.SetWinners(It.IsAny<List<Guid>>(), It.IsAny<int>()),
            Times.Never,
            "Consecutive pass counter should reset after tile exchange");
    }

    // -----------------------------------------------------------------------
    // Three-player consecutive passes
    // -----------------------------------------------------------------------

    [Fact]
    public void SkipTurn_ThreePlayersAllPass_EndsGame()
    {
        // Arrange
        var manager = CreateManager(ThreePlayerState());
        SetupPlayersForEndGame(_playerOneId, _playerTwoId, _playerThreeId);

        // Act — all 3 players skip consecutively
        manager.SkipTurn(_playerOneId);
        manager.SkipTurn(_playerTwoId);
        manager.SkipTurn(_playerThreeId);

        // Assert
        _mockEndGameManager.Verify(
            x => x.SetWinners(It.IsAny<List<Guid>>(), It.IsAny<int>()),
            Times.Once,
            "Game should end when all 3 players pass consecutively");
    }

    [Fact]
    public void SkipTurn_TwoOfThreePlayersPass_DoesNotEndGame()
    {
        // Arrange
        var manager = CreateManager(ThreePlayerState());

        // Act — only 2 of 3 players skip
        manager.SkipTurn(_playerOneId);
        manager.SkipTurn(_playerTwoId);

        // Assert — not all players have passed
        _mockEndGameManager.Verify(
            x => x.SetWinners(It.IsAny<List<Guid>>(), It.IsAny<int>()),
            Times.Never,
            "Game should not end when only 2 of 3 players have passed");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private PlayerMoveManager CreateManager(PlayerMoveStateModel state)
    {
        var manager = new PlayerMoveManager(
            _mockBagManager.Object,
            _mockBoardManager.Object,
            _mockPlayerManager.Object,
            _mockEndGameManager.Object);
        manager.InitFromStateModel(state);
        return manager;
    }

    private PlayerMoveStateModel TwoPlayerState()
    {
        return new PlayerMoveStateModel
        {
            CurrentPlayerId = _playerOneId,
            PlayerOrderIds = [_playerOneId, _playerTwoId],
            TurnsTaken = 0,
            LastMoveType = LastMoveType.Null,
            LastMoveResult = null
        };
    }

    private PlayerMoveStateModel ThreePlayerState()
    {
        return new PlayerMoveStateModel
        {
            CurrentPlayerId = _playerOneId,
            PlayerOrderIds = [_playerOneId, _playerTwoId, _playerThreeId],
            TurnsTaken = 0,
            LastMoveType = LastMoveType.Null,
            LastMoveResult = null
        };
    }

    private void SetupPlayersForEndGame(params Guid[] playerIds)
    {
        var players = new List<IPlayer>();
        foreach (var id in playerIds)
        {
            var mockPlayer = new Mock<IPlayer>();
            mockPlayer.Setup(p => p.PlayerId).Returns(id);
            mockPlayer.Setup(p => p.Score).Returns(0);
            mockPlayer.Setup(p => p.Tiles).Returns(new List<Tile>());
            _mockPlayerManager.Setup(x => x.GetPlayer(id)).Returns(mockPlayer.Object);
            players.Add(mockPlayer.Object);
        }
        _mockPlayerManager.Setup(x => x.GetPlayers()).Returns(players);
        _mockPlayerManager.Setup(x => x.Players).Returns(players);
    }

    private Player SetupPlayerForMove(Guid playerId)
    {
        var player = new Player();
        player.InitNew(playerId, "TestPlayer");
        player.SetTiles(new List<Tile>
        {
            new() { TileId = Guid.NewGuid(), Letter = "A", RackPosition = 0 },
            new() { TileId = Guid.NewGuid(), Letter = "B", RackPosition = 1 },
            new() { TileId = Guid.NewGuid(), Letter = "C", RackPosition = 2 }
        });
        _mockPlayerManager.Setup(x => x.GetPlayer(playerId)).Returns(player);
        return player;
    }

    private static PlayerMove CreateValidMove(Guid playerId, Player player)
    {
        player.Tiles[0].RackPosition = -1;
        return new PlayerMove
        {
            PlayerId = playerId,
            Tiles = [player.Tiles[0]]
        };
    }
}
