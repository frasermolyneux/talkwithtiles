using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.CoreEngine.Game;
using Xunit;

namespace MX.TalkWithTiles.CoreEngine.Tests.Game;
public class PlayerMoveManagerTests
{
    // This is the current player
    private readonly Guid _playerOneId = Guid.NewGuid();

    // This is the last player and also the next player
    private readonly Guid _playerTwoId = Guid.NewGuid();

    private readonly Mock<IBagManager> _mockBagManager;
    private readonly Mock<IBoardManager> _mockBoardManager;
    private readonly Mock<IEndGameManager> _mockEndGameManager;
    private readonly Mock<IPlayerManager> _mockPlayerManager;

    public PlayerMoveManagerTests()
    {
        _mockBagManager = new Mock<IBagManager>();
        _mockBoardManager = new Mock<IBoardManager>();
        _mockEndGameManager = new Mock<IEndGameManager>();
        _mockPlayerManager = new Mock<IPlayerManager>();
    }

    [Fact]
    public void InitNew()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);

        // Act
        playerMoveManager.InitNew();

        // Assert
        var result = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(result);
        Assert.Equal(0, result.TurnsTaken);
        Assert.Equal(LastMoveType.Null, result.LastMoveType);
        Assert.Null(result.LastMoveResult);
    }

    [Fact]
    public void InitFromStateModel()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);

        // Assert
        var result = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(result);
        Assert.Equal(defaultPlayerMoveStateModel.CurrentPlayerId, result.CurrentPlayerId);
        Assert.Equivalent(defaultPlayerMoveStateModel.PlayerOrderIds, result.PlayerOrderIds);
        Assert.Equal(defaultPlayerMoveStateModel.TurnsTaken, result.TurnsTaken);
        Assert.Equal(defaultPlayerMoveStateModel.LastMoveType, result.LastMoveType);
        Assert.Equivalent(defaultPlayerMoveStateModel.LastMoveResult, result.LastMoveResult);
    }

    [Fact]
    public void UndoLastTurn()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        DefaultPlayer(_playerTwoId);

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        playerMoveManager.UndoLastTurn(_playerTwoId);

        // Assert
        var result = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(result);
        Assert.Equal(LastMoveType.UndoTurn, result.LastMoveType);

        _mockPlayerManager.Verify(x => x.RemoveFromScore(_playerTwoId,
            defaultPlayerMoveStateModel.LastMoveResult!.Points), Times.Once);

        _mockBagManager.Verify(x => x.ReturnTilesToBag(It.IsAny<List<Tile>>()), Times.Once);

        _mockBoardManager.Verify(x => x.UndoMove(It.IsAny<List<Tile>>()), Times.Once);

        Assert.Equal(_playerTwoId, result.CurrentPlayerId);

        Assert.Null(result.LastMoveResult);
    }

    [Fact]
    public void ExchangeTiles()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        var player = DefaultPlayer(_playerOneId);

        _mockBagManager.Setup(x => x.TakeTiles(2)).Returns(new List<Tile>
        {
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "D"
            },
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "E"
            }
        });

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        var exchangedTilesIds = player.Tiles.Take(2).Select(t => t.TileId).ToList();
        playerMoveManager.ExchangeTiles(defaultPlayerMoveStateModel.CurrentPlayerId, exchangedTilesIds);

        // Assert
        var result = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(result);

        _mockBagManager.Verify(x => x.TakeTiles(2), Times.Once);
        _mockBagManager.Verify(x => x.ReturnTilesToBag(It.IsAny<List<Tile>>()), Times.Once);

        Assert.Equal(3, player.Tiles.Count);
        Assert.DoesNotContain(player.Tiles, t => t.Letter == "A" || t.Letter == "B");
        Assert.Equal(2, player.Tiles.Count(t => t.Letter == "D" || t.Letter == "E"));
        Assert.DoesNotContain(player.Tiles, t => exchangedTilesIds.Contains(t.TileId));

        Assert.Equal(_playerTwoId, result.CurrentPlayerId);
    }

    [Fact]
    public void SkipTurn()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        playerMoveManager.SkipTurn(_playerOneId);

        // Assert
        var result = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(result);
        Assert.Equal(LastMoveType.SkippedTurn, result.LastMoveType);
        Assert.Equal(_playerTwoId, result.CurrentPlayerId);
    }

    [Fact]
    public void MakeMoveNotYourTurn()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        var playerMove = new PlayerMove
        {
            PlayerId = _playerTwoId,
            Tiles = new List<Tile>()
        };

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        var result = playerMoveManager.MakeMove(playerMove, false);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Equal("It is not your move", result.InvalidMessage);
    }

    [Fact]
    public void MakeMovePreventCheating()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        var player = DefaultPlayer(_playerOneId);

        player.Tiles[0].RackPosition = -1;

        var playerMove = new PlayerMove
        {
            PlayerId = player.PlayerId,
            Tiles = new List<Tile>
            {
                player.Tiles[0],
                new()
                {
                    TileId = Guid.NewGuid(),
                    Letter = "Z",
                    RackPosition = -1
                }
            }
        };

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        var result = playerMoveManager.MakeMove(playerMove, false);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Equal("Tiles have been placed that are not in your rack.", result.InvalidMessage);
    }

    [Fact]
    public void MakeMoveNotCurrentPlayerDryRun()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        var player = DefaultPlayer(_playerTwoId);

        player.Tiles[0].RackPosition = -1;
        player.Tiles[1].RackPosition = -1;

        var playerMove = new PlayerMove
        {
            PlayerId = player.PlayerId,
            Tiles = new List<Tile>
            {
                player.Tiles[0],
                player.Tiles[1]
            }
        };

        var playerMoveResult = new PlayerMoveResult(_playerTwoId);
        _mockBoardManager.Setup(x => x.MakeMove(playerMove)).Returns(playerMoveResult);

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        var result = playerMoveManager.MakeMove(playerMove, true);

        // Assert
        Assert.NotNull(result);

        _mockBoardManager.Verify(x => x.MakeMove(playerMove), Times.Once);
    }

    [Fact]
    public void MakeMoveCurrentPlayerInvalid()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        var player = DefaultPlayer(_playerOneId);

        player.Tiles[0].RackPosition = -1;
        player.Tiles[1].RackPosition = -1;

        var playerMove = new PlayerMove
        {
            PlayerId = player.PlayerId,
            Tiles = new List<Tile>
            {
                player.Tiles[0],
                player.Tiles[1]
            }
        };

        var playerMoveResult = new PlayerMoveResult(_playerOneId)
        {
            InvalidMessage = "Invalid Move"
        };

        _mockBoardManager.Setup(x => x.MakeMove(playerMove)).Returns(playerMoveResult);

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        var moveResult = playerMoveManager.MakeMove(playerMove, false);

        // Assert
        Assert.NotNull(moveResult);
        Assert.Equivalent(playerMoveResult, moveResult);

        _mockBoardManager.Verify(x => x.MakeMove(playerMove), Times.Once);

        var stateModelResult = playerMoveManager.PlayerMoveStateModel;

        Assert.Equal(4, stateModelResult.TurnsTaken);
    }

    [Fact]
    public void MakeMoveCurrentPlayerNormal()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        var player = DefaultPlayer(_playerOneId);

        player.Tiles[0].RackPosition = -1;
        player.Tiles[1].RackPosition = -1;

        var playerMove = new PlayerMove
        {
            PlayerId = player.PlayerId,
            Tiles = new List<Tile>
            {
                player.Tiles[0],
                player.Tiles[1]
            }
        };

        var playerMoveResult = new PlayerMoveResult(_playerOneId);

        _mockBoardManager.Setup(x => x.MakeMove(playerMove)).Returns(playerMoveResult);

        _mockBagManager.Setup(x => x.TakeTiles(It.IsAny<int>())).Returns(new List<Tile>
        {
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "D"
            },
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "E"
            }
        });

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        var moveResult = playerMoveManager.MakeMove(playerMove, false);

        // Assert
        Assert.NotNull(moveResult);
        Assert.Equivalent(playerMoveResult, moveResult);

        _mockBoardManager.Verify(x => x.MakeMove(playerMove), Times.Once);

        var stateModelResult = playerMoveManager.PlayerMoveStateModel;

        Assert.Equal(5, stateModelResult.TurnsTaken);
    }

    [Fact]
    public void MakeMoveCurrentPlayerEndGame()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        var playerOne = DefaultPlayer(_playerOneId);
        var playerTwo = DefaultPlayer(_playerTwoId);

        playerOne.Tiles[0].RackPosition = -1;
        playerOne.Tiles[1].RackPosition = -1;
        playerOne.Tiles[2].RackPosition = -1;

        var playerMove = new PlayerMove
        {
            PlayerId = playerOne.PlayerId,
            Tiles = new List<Tile>
            {
                playerOne.Tiles[0],
                playerOne.Tiles[1],
                playerOne.Tiles[2]
            }
        };

        var playerMoveResult = new PlayerMoveResult(_playerOneId)
        {
            InvalidMessage = null,
            Points = 115
        };

        _mockBoardManager.Setup(x => x.MakeMove(playerMove)).Returns(playerMoveResult);
        _mockBagManager.Setup(x => x.TakeTiles(It.IsAny<int>())).Returns(new List<Tile>());
        _mockPlayerManager.Setup(x => x.Players).Returns(new List<IPlayer> {playerOne, playerTwo});
        _mockPlayerManager.Setup(x => x.GetPlayers()).Returns(new List<IPlayer> {playerOne, playerTwo});
        _mockBoardManager.Setup(x => x.LetterValue(It.IsAny<string>())).Returns(1);

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        var moveResult = playerMoveManager.MakeMove(playerMove, false);

        // Assert
        Assert.NotNull(moveResult);
        Assert.Equivalent(playerMoveResult, moveResult);

        _mockBoardManager.Verify(x => x.MakeMove(playerMove), Times.Once);

        var stateModelResult = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(stateModelResult);
        Assert.Equal(5, stateModelResult.TurnsTaken);
        Assert.Equal(_playerTwoId, stateModelResult.CurrentPlayerId);

        _mockPlayerManager.Verify(x => x.AddToScore(_playerOneId, 115), Times.Once);

        Assert.Equivalent(playerMoveResult, stateModelResult.LastMoveResult);

        _mockPlayerManager.Verify(x => x.AddToScore(_playerOneId, 3), Times.Once);
        _mockEndGameManager.Verify(x => x.SetWinners(It.IsAny<List<Guid>>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void SetLastMoveToChallengeResolved()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        playerMoveManager.SetLastMovedToChallengeResolved();

        // Assert
        var result = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(result);
        Assert.Equal(LastMoveType.ChallengeResolved, result.LastMoveType);
    }

    [Fact]
    public void SetNextPlayer()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        playerMoveManager.SetNextPlayer();

        // Assert
        var result = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(result);
        Assert.Equal(_playerTwoId, result.CurrentPlayerId);
    }

    [Fact]
    public void SetRandomPlayerOrder()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        var playerOne = DefaultPlayer(_playerOneId);
        var playerTwo = DefaultPlayer(_playerTwoId);

        _mockPlayerManager.Setup(x => x.GetPlayers()).Returns(new List<IPlayer> { playerOne, playerTwo });

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        playerMoveManager.SetRandomPlayerOrder();

        // Assert
        var result = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.CurrentPlayerId);
        Assert.NotNull(result.PlayerOrderIds);
        Assert.NotEmpty(result.PlayerOrderIds);
    }

    [Fact]
    public void SetCurrentPlayer()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        playerMoveManager.SetCurrentPlayer(_playerTwoId);

        // Assert
        var result = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(result);
        Assert.Equal(_playerTwoId, result.CurrentPlayerId);
    }

    [Fact]
    public void UpdateLastMove()
    {
        // Arrange
        var playerMoveManager = new PlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object);
        var defaultPlayerMoveStateModel = DefaultPlayerMoveStateModel();

        var playerMoveResult = new PlayerMoveResult(_playerOneId)
        {
            InvalidMessage = null,
            Points = 115
        };

        // Act
        playerMoveManager.InitFromStateModel(defaultPlayerMoveStateModel);
        playerMoveManager.UpdateLastMove(playerMoveResult);

        // Assert
        var result = playerMoveManager.PlayerMoveStateModel;

        Assert.NotNull(result);
        Assert.Equivalent(playerMoveResult, result.LastMoveResult);
    }

    private PlayerMoveStateModel DefaultPlayerMoveStateModel()
    {
        return new()
        {
            CurrentPlayerId = _playerOneId,
            LastMoveResult = new PlayerMoveResult(_playerTwoId)
            {
                NextPlayer = _playerOneId,
                Points = 30,
                Tiles = new List<Tile>(),
                WordsAndPoints = new List<WordAndScore>
                {
                    new()
                    {
                        Score = 30,
                        Word = "Henry"
                    }
                }
            },
            LastMoveType = LastMoveType.Normal,
            PlayerOrderIds = new List<Guid>
            {
                _playerOneId,
                _playerTwoId
            },
            TurnsTaken = 4
        };
    }

    private Player DefaultPlayer(Guid playerId)
    {
        var player = new Player();
        player.InitNew(playerId, "Tom");

        var playerTiles = new List<Tile>
        {
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "A",
                RackPosition = 1
            },
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "B",
                RackPosition = 2
            },
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "C",
                RackPosition = 3
            }
        };
        player.SetTiles(playerTiles);

        _mockPlayerManager.Setup(x => x.GetPlayer(playerId)).Returns(player);

        return player;
    }
}
