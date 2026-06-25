using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.CoreEngine.Game;
using Xunit;

namespace MX.TalkWithTiles.CoreEngine.Tests.Game;

public class EndGameManagerTests
{
    [Fact]
    public void InitNew()
    {
        // Arrange
        var endGameManager = new EndGameManager();

        // Act
        endGameManager.InitNew();

        // Assert
        var result = endGameManager.EndGameStateModel;

        Assert.Equal(GameStateType.InProgress, result.GameStateType);
        Assert.Equivalent(new Dictionary<Guid, int>(), result.SkippedTurns);
    }

    [Fact]
    public void InitFromStateModelAbandoned()
    {
        // Arrange
        var endGameManager = new EndGameManager();
        var abandoningPlayerId = Guid.NewGuid();

        // Act
        endGameManager.InitFromStateModel(new EndGameStateModel
        {
            GameStateType = GameStateType.Abandoned,
            SkippedTurns = new Dictionary<Guid, int>(),
            AbandoningPlayerId = abandoningPlayerId
        });

        // Assert
        var result = endGameManager.EndGameStateModel;

        Assert.Equal(GameStateType.Abandoned, result.GameStateType);
        Assert.Equal(abandoningPlayerId, result.AbandoningPlayerId);
    }

    [Fact]
    public void InitFromStateModelCompleted()
    {
        // Arrange
        var endGameManager = new EndGameManager();

        // Act
        endGameManager.InitFromStateModel(new EndGameStateModel
        {
            GameStateType = GameStateType.Completed,
            SkippedTurns = new Dictionary<Guid, int>(),
            Winners = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            }
        });

        // Assert
        var result = endGameManager.EndGameStateModel;

        Assert.Equal(GameStateType.Completed, result.GameStateType);
        Assert.Equal(2, result.Winners.Count);
    }

    [Fact]
    public void SetWinners()
    {
        // Arrange
        var endGameManager = new EndGameManager();

        var winningPlayerIds = new List<Guid>
        {
            Guid.NewGuid(), Guid.NewGuid()
        };

        // Act
        endGameManager.InitFromStateModel(new EndGameStateModel
        {
            GameStateType = GameStateType.InProgress,
            SkippedTurns = new Dictionary<Guid, int>()
        });

        endGameManager.SetWinners(winningPlayerIds, 555);

        // Assert
        var result = endGameManager.EndGameStateModel;

        Assert.Equal(GameStateType.Completed, result.GameStateType);
        Assert.Equivalent(winningPlayerIds, result.Winners);
        Assert.Equal(555, result.WinnerPoints);
    }

    [Fact]
    public void AbandonGame()
    {
        // Arrange
        var endGameManager = new EndGameManager();
        var abandoningPlayerId = Guid.NewGuid();

        // Act
        endGameManager.InitFromStateModel(new EndGameStateModel
        {
            GameStateType = GameStateType.InProgress,
            SkippedTurns = new Dictionary<Guid, int>()
        });

        endGameManager.AbandonGame(abandoningPlayerId);

        // Assert
        var result = endGameManager.EndGameStateModel;

        Assert.Equal(GameStateType.Abandoned, result.GameStateType);
        Assert.Equal(abandoningPlayerId, result.AbandoningPlayerId);
    }
}
