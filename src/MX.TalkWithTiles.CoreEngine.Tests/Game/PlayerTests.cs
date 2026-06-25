using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.CoreEngine.Game;
using Xunit;

namespace MX.TalkWithTiles.CoreEngine.Tests.Game;

public class PlayerTests
{
    [Fact]
    public void InitNew()
    {
        // Arrange
        var player = new Player();
        var playerId = Guid.NewGuid();

        // Act
        player.InitNew(playerId, "Henry");

        // Assert
        var result = player.PlayerStateModel;

        Assert.Equal(playerId, result.PlayerId);
        Assert.Equal("Henry", result.PlayerName);
        Assert.Empty(result.Tiles);
        Assert.Empty(result.NewTiles);
    }

    [Fact]
    public void InitFromStateModel()
    {
        // Arrange
        var player = new Player();
        var defaultPlayerStateModel = DefaultPlayerStateModel();

        // Act
        player.InitFromStateModel(defaultPlayerStateModel);

        // Assert
        var result = player.PlayerStateModel;

        Assert.Equal(defaultPlayerStateModel.PlayerId, result.PlayerId);
        Assert.Equal(defaultPlayerStateModel.PlayerName, result.PlayerName);
        Assert.Equivalent(defaultPlayerStateModel.Tiles, result.Tiles);
        Assert.Equivalent(defaultPlayerStateModel.NewTiles, result.NewTiles);
        Assert.Equal(defaultPlayerStateModel.Score, result.Score);
    }

    [Fact]
    public void SetTiles()
    {
        // Arrange
        var player = new Player();
        var defaultPlayerStateModel = DefaultPlayerStateModel();

        var playerTiles = new List<Tile>
        {
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "X"
            },
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "Y"
            },
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "Z"
            }
        };

        // Act
        player.InitFromStateModel(defaultPlayerStateModel);
        player.SetTiles(playerTiles);

        // Assert
        var result = player.PlayerStateModel;

        Assert.Equal(defaultPlayerStateModel.PlayerId, result.PlayerId);
        Assert.Equal(defaultPlayerStateModel.PlayerName, result.PlayerName);
        Assert.Equivalent(playerTiles, result.Tiles);
        Assert.Equivalent(defaultPlayerStateModel.NewTiles, result.NewTiles);
        Assert.Equal(defaultPlayerStateModel.Score, result.Score);
    }

    [Fact]
    public void AddToScore()
    {
        // Arrange
        var player = new Player();
        var defaultPlayerStateModel = DefaultPlayerStateModel();

        // Act
        player.InitFromStateModel(defaultPlayerStateModel);
        player.AddToScore(50);

        // Assert
        var result = player.PlayerStateModel;

        Assert.Equal(defaultPlayerStateModel.PlayerId, result.PlayerId);
        Assert.Equal(defaultPlayerStateModel.PlayerName, result.PlayerName);
        Assert.Equivalent(defaultPlayerStateModel.Tiles, result.Tiles);
        Assert.Equivalent(defaultPlayerStateModel.NewTiles, result.NewTiles);
        Assert.Equal(defaultPlayerStateModel.Score + 50, result.Score);
    }

    [Fact]
    public void UpdateInvitedPlayer()
    {
        // Arrange
        var player = new Player();
        var defaultPlayerStateModel = DefaultPlayerStateModel();

        var newPlayerId = Guid.NewGuid();

        // Act
        player.InitFromStateModel(defaultPlayerStateModel);
        player.UpdateInvitedPlayer(newPlayerId, "Olu Returns");

        // Assert
        var result = player.PlayerStateModel;

        Assert.Equal(newPlayerId, result.PlayerId);
        Assert.Equal("Olu Returns", result.PlayerName);
        Assert.Equivalent(defaultPlayerStateModel.Tiles, result.Tiles);
        Assert.Equivalent(defaultPlayerStateModel.NewTiles, result.NewTiles);
        Assert.Equal(defaultPlayerStateModel.Score, result.Score);
    }

    [Fact]
    public void RemoveFromScore()
    {
        // Arrange
        var player = new Player();
        var defaultPlayerStateModel = DefaultPlayerStateModel();

        // Act
        player.InitFromStateModel(defaultPlayerStateModel);
        player.RemoveFromScore(50);

        // Assert
        var result = player.PlayerStateModel;

        Assert.Equal(defaultPlayerStateModel.PlayerId, result.PlayerId);
        Assert.Equal(defaultPlayerStateModel.PlayerName, result.PlayerName);
        Assert.Equivalent(defaultPlayerStateModel.Tiles, result.Tiles);
        Assert.Equivalent(defaultPlayerStateModel.NewTiles, result.NewTiles);
        Assert.Equal(defaultPlayerStateModel.Score - 50, result.Score);
    }

    [Fact]
    public void SetNewTiles()
    {
        // Arrange
        var player = new Player();
        var defaultPlayerStateModel = DefaultPlayerStateModel();

        var newPlayerTiles = new List<Tile>
        {
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "X"
            },
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "Y"
            },
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "Z"
            }
        };

        // Act
        player.InitFromStateModel(defaultPlayerStateModel);
        player.SetNewTiles(newPlayerTiles);

        // Assert
        var result = player.PlayerStateModel;

        Assert.Equal(defaultPlayerStateModel.PlayerId, result.PlayerId);
        Assert.Equal(defaultPlayerStateModel.PlayerName, result.PlayerName);
        Assert.Equivalent(defaultPlayerStateModel.Tiles, result.Tiles);
        Assert.Equivalent(newPlayerTiles, result.NewTiles);
        Assert.Equal(defaultPlayerStateModel.Score, result.Score);
    }

    private PlayerStateModel DefaultPlayerStateModel()
    {
        var playerId = Guid.NewGuid();
        var playerTiles = new List<Tile>
        {
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "A"
            },
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "B"
            },
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "C"
            }
        };
        var newTiles = new List<Tile>
        {
            new()
            {
                TileId = Guid.NewGuid(),
                Letter = "C"
            }
        };

        return new PlayerStateModel
        {
            PlayerId = playerId,
            PlayerName = "Olu",
            Tiles = playerTiles,
            NewTiles = newTiles,
            Score = 999
        };
    }
}
