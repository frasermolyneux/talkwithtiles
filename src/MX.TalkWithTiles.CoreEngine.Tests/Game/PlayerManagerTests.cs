using System;
using System.Collections.Generic;
using Moq;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.CoreEngine.Game;
using Xunit;

namespace MX.TalkWithTiles.CoreEngine.Tests.Game;
public class PlayerManagerTests
{
    private readonly Mock<IPlayerFactory> _mockPlayerFactory;

    public PlayerManagerTests()
    {
        _mockPlayerFactory = new Mock<IPlayerFactory>();
        _mockPlayerFactory.Setup(x => x.CreateFromStateModel(It.IsAny<PlayerStateModel>()))
            .Returns((PlayerStateModel psm) =>
            {
                var mockPlayer = new Mock<IPlayer>();
                mockPlayer.Setup(p => p.PlayerStateModel).Returns(psm);
                mockPlayer.Setup(p => p.PlayerId).Returns(psm.PlayerId);
                return mockPlayer.Object;
            });
        _mockPlayerFactory.Setup(x => x.CreateNew(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns((Guid id, string name) =>
            {
                var mockPlayer = new Mock<IPlayer>();
                mockPlayer.Setup(p => p.PlayerStateModel).Returns(new PlayerStateModel { PlayerId = id, PlayerName = name });
                mockPlayer.Setup(p => p.PlayerId).Returns(id);
                return mockPlayer.Object;
            });
    }

    [Fact]
    public void InitNew()
    {
        // Arrange
        var playerManager = new PlayerManager(_mockPlayerFactory.Object);

        // Act
        playerManager.InitNew();

        // Assert
        var result = playerManager.PlayersStateModel;

        Assert.NotNull(result.Players);
        Assert.Empty(result.Players);
    }

    [Fact]
    public void InitFromStateModel()
    {
        // Arrange
        var playerManager = new PlayerManager(_mockPlayerFactory.Object);
        var defaultPlayersStateModel = DefaultPlayersStateModel();

        // Act
        playerManager.InitFromStateModel(defaultPlayersStateModel);

        // Assert
        var result = playerManager.PlayersStateModel;

        Assert.Equal(3, result.Players.Count);

        _mockPlayerFactory.Verify(x => x.CreateFromStateModel(It.IsAny<PlayerStateModel>()), Times.Exactly(3));
    }

    [Fact]
    public void AddPlayer()
    {
        // Arrange
        var playerManager = new PlayerManager(_mockPlayerFactory.Object);
        var defaultPlayersStateModel = DefaultPlayersStateModel();

        // Act
        playerManager.InitFromStateModel(defaultPlayersStateModel);
        playerManager.AddPlayer(Guid.NewGuid(), "New Player");

        // Assert
        var result = playerManager.PlayersStateModel;

        Assert.Equal(4, result.Players.Count);

        _mockPlayerFactory.Verify(x => x.CreateNew(It.IsAny<Guid>(), It.IsAny<string>()), Times.Exactly(1));
    }

    private static PlayersStateModel DefaultPlayersStateModel()
    {
        return new()
        {
            Players = new List<PlayerStateModel>
            {
                new()
                {
                    PlayerId = Guid.NewGuid(),
                    PlayerName = "Olu"
                },
                new()
                {
                    PlayerId = Guid.NewGuid(),
                    PlayerName = "Henry"
                },
                new()
                {
                    PlayerId = Guid.NewGuid(),
                    PlayerName = "Simon"
                }
            }
        };
    }
}
