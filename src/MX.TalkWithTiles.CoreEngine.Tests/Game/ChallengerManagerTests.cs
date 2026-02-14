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
public class ChallengerManagerTests
{
    private readonly Mock<IPlayerMoveManager> _mockPlayerMoveManager;

    public ChallengerManagerTests()
    {
        _mockPlayerMoveManager = new Mock<IPlayerMoveManager>();
    }

    [Fact]
    public void InitNew()
    {
        // Arrange
        var challengeManager = new ChallengeManager(_mockPlayerMoveManager.Object);

        // Act
        challengeManager.InitNew(false, DefaultChallengeStateModel().ChallengeResults);

        // Assert
        var result = challengeManager.ChallengeStateModel;

        Assert.False(result.CanOverrideChallengeOutcome);
        Assert.Equivalent(DefaultChallengeStateModel().ChallengeResults, result.ChallengeResults);
        Assert.Equal(Guid.Empty, result.ChallengedPlayerId);
        Assert.Equal(Guid.Empty, result.ChallengerPlayerId);
        Assert.Null(result.ChallengeReason);
        Assert.Empty(result.ChallengeText);
        Assert.Null(result.PlayerChallengeResult);
    }

    [Fact]
    public void InitFromStateModel()
    {
        // Arrange
        var challengeManager = new ChallengeManager(_mockPlayerMoveManager.Object);

        // Act
        var challengeStateModel = DefaultChallengeStateModel();
        challengeManager.InitFromStateModel(challengeStateModel);

        // Assert
        var result = challengeManager.ChallengeStateModel;

        Assert.True(result.CanOverrideChallengeOutcome);
        Assert.Equivalent(DefaultChallengeStateModel().ChallengeResults, result.ChallengeResults);
        Assert.Equal(Guid.Empty, result.ChallengedPlayerId);
        Assert.Equal(Guid.Empty, result.ChallengerPlayerId);
        Assert.Null(result.ChallengeReason);
        Assert.Empty(result.ChallengeText);
        Assert.Null(result.PlayerChallengeResult);
    }

    [Fact]
    public void IssuePlayerChallenge()
    {
        // Arrange
        var challengeManager = new ChallengeManager(_mockPlayerMoveManager.Object);

        var lastPlayerId = Guid.NewGuid();
        var lastPlayerMoveResult = new PlayerMoveResult(lastPlayerId);
        _mockPlayerMoveManager.Setup(x => x.LastMoveResult).Returns(lastPlayerMoveResult);

        var challengerPlayerId = Guid.NewGuid();

        // Act
        var challengeStateModel = DefaultChallengeStateModel();
        challengeManager.InitFromStateModel(challengeStateModel);
        challengeManager.IssuePlayerChallenge(challengerPlayerId, GameChallengeReason.ThatsNotAValidTurn,
            "This is my challenge");

        // Assert
        var result = challengeManager.ChallengeStateModel;

        Assert.Equal(lastPlayerId, result.ChallengedPlayerId);
        Assert.Equal(challengerPlayerId, result.ChallengerPlayerId);
        Assert.Equal(GameChallengeReason.ThatsNotAValidTurn, result.ChallengeReason);
        Assert.Equal("This is my challenge", result.ChallengeText);
        Assert.Null(result.PlayerChallengeResult);
    }

    [Fact]
    public void ResolveChallengeAcceptedRetryPlayerMove()
    {
        // Arrange
        var challengeManager = new ChallengeManager(_mockPlayerMoveManager.Object);

        var lastPlayerId = Guid.NewGuid();
        var lastPlayerMoveResult = new PlayerMoveResult(lastPlayerId);
        _mockPlayerMoveManager.Setup(x => x.LastMoveResult).Returns(lastPlayerMoveResult);

        var challengerPlayerId = Guid.NewGuid();

        // Act
        var challengeStateModel = DefaultChallengeStateModel();
        challengeManager.InitFromStateModel(challengeStateModel);
        challengeManager.IssuePlayerChallenge(challengerPlayerId, GameChallengeReason.Catchall,
            "This is my challenge");
        challengeManager.ResolveChallenge(true, null);

        // Assert
        var result = challengeManager.ChallengeStateModel;

        Assert.Equal(Guid.Empty, result.ChallengedPlayerId);
        Assert.Equal(Guid.Empty, result.ChallengerPlayerId);
        Assert.Null(result.ChallengeReason);
        Assert.Empty(result.ChallengeText);

        Assert.NotNull(result.PlayerChallengeResult);
        Assert.Equal(lastPlayerId, result.PlayerChallengeResult.ChallengedPlayerId);
        Assert.Equal(challengerPlayerId, result.PlayerChallengeResult.ChallengerPlayerId);
        Assert.Equal(GameChallengeReason.Catchall, result.PlayerChallengeResult.GameChallengeReason);
        Assert.Equal(GameChallengeResult.RetryPlayerMove, result.PlayerChallengeResult.GameChallengeResult);

        _mockPlayerMoveManager.Verify(x => x.SetLastMovedToChallengeResolved(), Times.Once);
        _mockPlayerMoveManager.Verify(x => x.UndoLastTurn(lastPlayerId), Times.Once);
    }

    [Fact]
    public void ResolveChallengeAcceptedLosePointsAndProceedToNextPlayer()
    {
        // Arrange
        var challengeManager = new ChallengeManager(_mockPlayerMoveManager.Object);

        var lastPlayerId = Guid.NewGuid();
        var lastPlayerMoveResult = new PlayerMoveResult(lastPlayerId);
        _mockPlayerMoveManager.Setup(x => x.LastMoveResult).Returns(lastPlayerMoveResult);

        var challengerPlayerId = Guid.NewGuid();

        // Act
        var challengeStateModel = DefaultChallengeStateModel();
        challengeManager.InitFromStateModel(challengeStateModel);
        challengeManager.IssuePlayerChallenge(challengerPlayerId, GameChallengeReason.ThatsNotAWord,
            "This is my challenge");
        challengeManager.ResolveChallenge(true, null);

        // Assert
        var result = challengeManager.ChallengeStateModel;

        Assert.Equal(Guid.Empty, result.ChallengedPlayerId);
        Assert.Equal(Guid.Empty, result.ChallengerPlayerId);
        Assert.Null(result.ChallengeReason);
        Assert.Empty(result.ChallengeText);

        Assert.NotNull(result.PlayerChallengeResult);
        Assert.Equal(lastPlayerId, result.PlayerChallengeResult.ChallengedPlayerId);
        Assert.Equal(challengerPlayerId, result.PlayerChallengeResult.ChallengerPlayerId);
        Assert.Equal(GameChallengeReason.ThatsNotAWord, result.PlayerChallengeResult.GameChallengeReason);
        Assert.Equal(GameChallengeResult.LosePointsAndProceedToNextPlayer, result.PlayerChallengeResult.GameChallengeResult);

        _mockPlayerMoveManager.Verify(x => x.SetLastMovedToChallengeResolved(), Times.Once);
        _mockPlayerMoveManager.Verify(x => x.UndoLastTurn(lastPlayerId), Times.Once);
        _mockPlayerMoveManager.Verify(x => x.SetNextPlayer(), Times.Once);
    }

    [Theory]
    [InlineData(GameChallengeReason.Catchall, GameChallengeResult.RetryPlayerMove)]
    [InlineData(GameChallengeReason.Catchall, GameChallengeResult.LosePointsAndProceedToNextPlayer)]
    [InlineData(GameChallengeReason.Catchall, GameChallengeResult.Nothing)]
    [InlineData(GameChallengeReason.ThatsNotAWord, GameChallengeResult.RetryPlayerMove)]
    [InlineData(GameChallengeReason.ThatsNotAWord, GameChallengeResult.LosePointsAndProceedToNextPlayer)]
    [InlineData(GameChallengeReason.ThatsNotAWord, GameChallengeResult.Nothing)]
    [InlineData(GameChallengeReason.ThatsNotAValidTurn, GameChallengeResult.RetryPlayerMove)]
    [InlineData(GameChallengeReason.ThatsNotAValidTurn, GameChallengeResult.LosePointsAndProceedToNextPlayer)]
    [InlineData(GameChallengeReason.ThatsNotAValidTurn, GameChallengeResult.Nothing)]
    public void ResolveChallengeAcceptedOverride(GameChallengeReason gameChallengeReason,
        GameChallengeResult gameChallengeResult)
    {
        // Arrange
        var challengeManager = new ChallengeManager(_mockPlayerMoveManager.Object);

        var lastPlayerId = Guid.NewGuid();
        var lastPlayerMoveResult = new PlayerMoveResult(lastPlayerId);
        _mockPlayerMoveManager.Setup(x => x.LastMoveResult).Returns(lastPlayerMoveResult);

        var challengerPlayerId = Guid.NewGuid();

        // Act
        var challengeStateModel = DefaultChallengeStateModel();
        challengeManager.InitFromStateModel(challengeStateModel);
        challengeManager.IssuePlayerChallenge(challengerPlayerId, gameChallengeReason, "This is my challenge");
        challengeManager.ResolveChallenge(true, gameChallengeResult);

        // Assert
        var result = challengeManager.ChallengeStateModel;

        Assert.Equal(Guid.Empty, result.ChallengedPlayerId);
        Assert.Equal(Guid.Empty, result.ChallengerPlayerId);
        Assert.Null(result.ChallengeReason);
        Assert.Empty(result.ChallengeText);

        Assert.NotNull(result.PlayerChallengeResult);
        Assert.Equal(lastPlayerId, result.PlayerChallengeResult.ChallengedPlayerId);
        Assert.Equal(challengerPlayerId, result.PlayerChallengeResult.ChallengerPlayerId);
        Assert.Equal(gameChallengeReason, result.PlayerChallengeResult.GameChallengeReason);
        Assert.Equal(gameChallengeResult, result.PlayerChallengeResult.GameChallengeResult);

        _mockPlayerMoveManager.Verify(x => x.SetLastMovedToChallengeResolved(), Times.Once);
    }

    [Theory]
    [InlineData(GameChallengeReason.Catchall)]
    [InlineData(GameChallengeReason.ThatsNotAWord)]
    [InlineData(GameChallengeReason.ThatsNotAValidTurn)]
    public void ResolveChallengeRejected(GameChallengeReason gameChallengeReason)
    {
        // Arrange
        var challengeManager = new ChallengeManager(_mockPlayerMoveManager.Object);

        var lastPlayerId = Guid.NewGuid();
        var lastPlayerMoveResult = new PlayerMoveResult(lastPlayerId);
        _mockPlayerMoveManager.Setup(x => x.LastMoveResult).Returns(lastPlayerMoveResult);

        var challengerPlayerId = Guid.NewGuid();

        // Act
        var challengeStateModel = DefaultChallengeStateModel();
        challengeManager.InitFromStateModel(challengeStateModel);
        challengeManager.IssuePlayerChallenge(challengerPlayerId, gameChallengeReason, "This is my challenge");
        challengeManager.ResolveChallenge(false, null);

        // Assert
        var result = challengeManager.ChallengeStateModel;

        Assert.Equal(Guid.Empty, result.ChallengedPlayerId);
        Assert.Equal(Guid.Empty, result.ChallengerPlayerId);
        Assert.Null(result.ChallengeReason);
        Assert.Empty(result.ChallengeText);

        Assert.NotNull(result.PlayerChallengeResult);
        Assert.Equal(lastPlayerId, result.PlayerChallengeResult.ChallengedPlayerId);
        Assert.Equal(challengerPlayerId, result.PlayerChallengeResult.ChallengerPlayerId);
        Assert.Equal(gameChallengeReason, result.PlayerChallengeResult.GameChallengeReason);
        Assert.Equal(GameChallengeResult.Nothing, result.PlayerChallengeResult.GameChallengeResult);

        _mockPlayerMoveManager.Verify(x => x.SetLastMovedToChallengeResolved(), Times.Once);
    }

    private ChallengeStateModel DefaultChallengeStateModel()
    {
        return new()
        {
            CanOverrideChallengeOutcome = true,
            ChallengeText = string.Empty,
            ChallengeResults = new Dictionary<GameChallengeReason, GameChallengeResult>
            {
                {GameChallengeReason.Catchall, GameChallengeResult.RetryPlayerMove},
                {GameChallengeReason.ThatsNotAWord, GameChallengeResult.LosePointsAndProceedToNextPlayer},
                {GameChallengeReason.ThatsNotAValidTurn, GameChallengeResult.Nothing}
            }
        };
    }
}
