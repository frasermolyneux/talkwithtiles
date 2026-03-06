using System;
using System.Collections.Generic;
using Moq;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using Xunit;

namespace MX.TalkWithTiles.CoreEngine.Tests;
public class GameEngineTests
{
    private readonly Mock<IBagManager> _mockBagManager;
    private readonly Mock<IBoardManager> _mockBoardManager;
    private readonly Mock<IChallengeManager> _mockChallengeManager;
    private readonly Mock<IEndGameManager> _mockEndGameManager;
    private readonly Mock<IManagerFactory> _mockManagerFactory;
    private readonly Mock<IPlayerManager> _mockPlayerManager;
    private readonly Mock<IPlayerMoveManager> _mockPlayerMoveManager;

    public GameEngineTests()
    {
        _mockManagerFactory = new Mock<IManagerFactory>();

        _mockBoardManager = new Mock<IBoardManager>();
        _mockBagManager = new Mock<IBagManager>();
        _mockPlayerManager = new Mock<IPlayerManager>();
        _mockEndGameManager = new Mock<IEndGameManager>();
        _mockChallengeManager = new Mock<IChallengeManager>();
        _mockPlayerMoveManager = new Mock<IPlayerMoveManager>();

        _mockManagerFactory.Setup(x => x.CreateBoardManager(It.IsAny<GameType>()))
            .Returns(_mockBoardManager.Object);
        _mockManagerFactory.Setup(x => x.CreateBoardManager(It.IsAny<GameType>(), It.IsAny<BoardStateModel>()))
            .Returns(_mockBoardManager.Object);
        _mockManagerFactory.Setup(x => x.CreateBagManager(It.IsAny<GameType>())).Returns(_mockBagManager.Object);
        _mockManagerFactory.Setup(x => x.CreateBagManager(It.IsAny<BagStateModel>())).Returns(_mockBagManager.Object);
        _mockManagerFactory.Setup(x => x.CreatePlayerManager())
            .Returns(_mockPlayerManager.Object);
        _mockManagerFactory.Setup(x => x.CreatePlayerManager(It.IsAny<PlayersStateModel>()))
            .Returns(_mockPlayerManager.Object);
        _mockManagerFactory.Setup(x => x.CreateEndGameManager())
            .Returns(_mockEndGameManager.Object);
        _mockManagerFactory.Setup(x => x.CreateEndGameManager(It.IsAny<EndGameStateModel>()))
            .Returns(_mockEndGameManager.Object);
        _mockManagerFactory.Setup(x =>
                x.CreateChallengeManager(It.IsAny<bool>(), It.IsAny<Dictionary<GameChallengeReason, GameChallengeResult>>(), _mockPlayerMoveManager.Object))
            .Returns(_mockChallengeManager.Object);
        _mockManagerFactory.Setup(x =>
                x.CreateChallengeManager(It.IsAny<ChallengeStateModel>(), _mockPlayerMoveManager.Object))
            .Returns(_mockChallengeManager.Object);
        _mockManagerFactory.Setup(x => x.CreatePlayerMoveManager(_mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object)).Returns(_mockPlayerMoveManager.Object);
        _mockManagerFactory.Setup(x => x.CreatePlayerMoveManager(It.IsAny<PlayerMoveStateModel>(), _mockBagManager.Object,
            _mockBoardManager.Object, _mockPlayerManager.Object, _mockEndGameManager.Object)).Returns(_mockPlayerMoveManager.Object);
    }

    [Theory]
    [InlineData(GamePrivacyType.Private, GameType.MiniBoard, TileBagVisibilityOption.OnlyShowRemainingCount, false)]
    [InlineData(GamePrivacyType.Public, GameType.StandardBoard, TileBagVisibilityOption.DoNotShowRemainingTiles,
        true)]
    [InlineData(GamePrivacyType.Private, GameType.SuperSizeBoard, TileBagVisibilityOption.ShowAfterFirstTurns, false)]
    [InlineData(GamePrivacyType.Public, GameType.MiniBoard, TileBagVisibilityOption.ShowRemainingTiles, true)]
    public void InitNew(GamePrivacyType gamePrivacyType, GameType gameType,
        TileBagVisibilityOption tileBagVisibilityOption, bool canOverrideChallengeOutcome)
    {
        // Arrange 
        var gameEngine = new GameEngine(_mockManagerFactory.Object);

        var challengeResults = new Dictionary<GameChallengeReason, GameChallengeResult>();

        // Act
        gameEngine.InitNew(gamePrivacyType, gameType, tileBagVisibilityOption,
            canOverrideChallengeOutcome, challengeResults);

        // Assert
        var result = gameEngine.GameStateModel;

        Assert.NotNull(result);
        Assert.Equal(gamePrivacyType, result.GamePrivacyType);
        Assert.Equal(gameType, result.GameType);
        Assert.Equal(tileBagVisibilityOption, result.TileBagVisibilityOption);
    }

    [Fact]
    public void InitFromStateModel()
    {
        // Arrange
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);

        // Assert
        var result = gameEngine.GameStateModel;

        Assert.NotNull(result);
        Assert.Equal(GamePrivacyType.Public, result.GamePrivacyType);
        Assert.Equal(GameType.StandardBoard, result.GameType);
        Assert.Equal(TileBagVisibilityOption.OnlyShowRemainingCount, result.TileBagVisibilityOption);
    }

    [Fact]
    public void AddPlayer()
    {
        // Arrange
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();
        var playerId = Guid.NewGuid();

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);
        gameEngine.AddPlayer(playerId, "Jerry");

        // Assert
        _mockPlayerManager.Verify(x => x.AddPlayer(playerId, "Jerry"), Times.Once);
    }

    [Fact]
    public void SetRandomPlayerOrder()
    {
        // Arrange 
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);
        gameEngine.SetRandomPlayerOrder();

        // Assert
        _mockPlayerMoveManager.Verify(x => x.SetRandomPlayerOrder(), Times.Once);
    }

    [Fact]
    public void MakeMove()
    {
        // Arrange 
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);
        gameEngine.MakeMove(new PlayerMove(), true);

        // Assert
        _mockPlayerMoveManager.Verify(x => x.MakeMove(It.IsAny<PlayerMove>(), true), Times.Once);
    }

    [Fact]
    public void SkipMove()
    {
        // Arrange
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();
        var playerId = Guid.NewGuid();

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);
        gameEngine.SkipMove(playerId);

        // Assert
        _mockPlayerMoveManager.Verify(x => x.SkipTurn(playerId), Times.Once);
    }

    [Fact]
    public void ExchangeTiles()
    {
        // Arrange
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();
        var playerId = Guid.NewGuid();
        var tileIds = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()};

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);
        gameEngine.ExchangeTiles(playerId, tileIds);

        // Assert
        _mockPlayerMoveManager.Verify(x => x.ExchangeTiles(playerId, tileIds), Times.Once);
    }

    [Fact]
    public void AbandonGame()
    {
        // Arrange
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();
        var playerId = Guid.NewGuid();

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);
        gameEngine.AbandonGame(playerId);

        // Assert
        _mockEndGameManager.Verify(x => x.AbandonGame(playerId), Times.Once);
    }

    [Fact]
    public void UpdateInvitedPlayer()
    {
        // Arrange
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();
        var oldPlayerId = Guid.NewGuid();
        var newPlayerId = Guid.NewGuid();
        const string newPlayerName = "Frank";

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);
        gameEngine.UpdateInvitedPlayer(oldPlayerId, newPlayerId, newPlayerName);

        // Assert
        _mockPlayerMoveManager.Verify(x => x.UpdateInvitedPlayer(oldPlayerId, newPlayerId, newPlayerName), Times.Once);
    }

    [Fact]
    public void IssuePlayerChallenge()
    {
        // Arrange
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();
        var playerId = Guid.NewGuid();
        const GameChallengeReason gameChallengeReason = GameChallengeReason.Catchall;
        const string challengeText = "This is my challenge";

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);
        gameEngine.IssuePlayerChallenge(playerId, gameChallengeReason, challengeText);

        // Assert
        _mockChallengeManager.Verify(x => x.IssuePlayerChallenge(playerId, gameChallengeReason, challengeText), Times.Once);
    }

    [Fact]
    public void ResolveChallenge()
    {
        // Arrange
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();
        const bool accepted = true;
        const GameChallengeResult gameChallengeResult = GameChallengeResult.RetryPlayerMove;

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);
        gameEngine.ResolveChallenge(accepted, gameChallengeResult);

        // Assert
        _mockChallengeManager.Verify(x => x.ResolveChallenge(accepted, gameChallengeResult), Times.Once);
    }

    [Fact]
    public void UndoLastTurn()
    {
        // Arrange
        var gameEngine = new GameEngine(_mockManagerFactory.Object);
        var defaultGameStateModel = DefaultGameStateModel();
        var playerId = Guid.NewGuid();

        // Act
        gameEngine.InitFromStateModel(defaultGameStateModel);
        gameEngine.UndoLastTurn(playerId);

        // Assert
        _mockPlayerMoveManager.Verify(x => x.UndoLastTurn(playerId), Times.Once);
    }

    private static GameStateModel DefaultGameStateModel()
    {
        return new()
        {
            GameId = Guid.NewGuid(),
            GamePrivacyType = GamePrivacyType.Public,
            GameType = GameType.StandardBoard,
            TileBagVisibilityOption = TileBagVisibilityOption.OnlyShowRemainingCount,
            BoardStateModel = new BoardStateModel(),
            BagStateModel = new BagStateModel(),
            PlayersStateModel = new PlayersStateModel(),
            EndGameStateModel = new EndGameStateModel(),
            ChallengeStateModel = new ChallengeStateModel(),
            PlayerMoveStateModel = new PlayerMoveStateModel()
        };
    }
}
