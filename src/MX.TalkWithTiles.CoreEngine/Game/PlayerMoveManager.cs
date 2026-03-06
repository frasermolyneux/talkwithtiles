using System;
using System.Collections.Generic;
using System.Linq;
using MX.TalkWithTiles.Common.Extensions;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.CoreEngine.Game;

public class PlayerMoveManager(
    IBagManager bagManager,
    IBoardManager boardManager,
    IPlayerManager playerManager,
    IEndGameManager endGameManager) : IPlayerMoveManager
{
    public Guid CurrentPlayerId { get; private set; }
    public List<Guid> PlayerOrderIds { get; private set; } = [];
    public int TurnsTaken { get; private set; }
    public LastMoveType LastMoveType { get; private set; }
    public PlayerMoveResult? LastMoveResult { get; private set; }

    public PlayerMoveStateModel PlayerMoveStateModel
        => new()
        {
            CurrentPlayerId = CurrentPlayerId,
            PlayerOrderIds = PlayerOrderIds,
            TurnsTaken = TurnsTaken,
            LastMoveType = LastMoveType,
            LastMoveResult = LastMoveResult
        };

    public void UndoLastTurn(Guid playerId)
    {
        if (LastMoveResult is null)
            throw new InvalidOperationException("Cannot undo: no previous move recorded.");

        LastMoveType = LastMoveType.UndoTurn;

        // Take back the points
        playerManager.RemoveFromScore(playerId, LastMoveResult.Points);

        // Put new tiles back in the bag
        var player = playerManager.GetPlayer(playerId);
        var newTiles = player.NewTiles;
        bagManager.ReturnTilesToBag(newTiles);

        // Take back the tiles into rack
        boardManager.UndoMove(LastMoveResult.Tiles);
        player.SetTiles(LastMoveResult.Tiles);

        // Reset current player
        SetCurrentPlayer(playerId);

        LastMoveResult = null;
    }

    public void ExchangeTiles(Guid playerId, IEnumerable<Guid> tileIds)
    {
        LastMoveType = LastMoveType.ExchangedTiles;

        var player = playerManager.GetPlayer(playerId);

        var playerTiles = player.Tiles;
        var tilesToRemove = playerTiles.Where(t => tileIds.Contains(t.TileId)).ToList();
        var tilesToRemain = playerTiles.Where(t => !tileIds.Contains(t.TileId)).ToList();

        var newTiles = bagManager.TakeTiles(tilesToRemove.Count);

        bagManager.ReturnTilesToBag(tilesToRemove);

        tilesToRemain = tilesToRemain.Concat(newTiles).ToList();

        player.SetTiles(tilesToRemain);

        SetNextPlayer();
    }

    public void SkipTurn(Guid playerId)
    {
        LastMoveType = LastMoveType.SkippedTurn;
        SetNextPlayer();
    }

    public PlayerMoveResult MakeMove(PlayerMove playerMove, bool dryRun)
    {
        LastMoveType = LastMoveType.Normal;

        if (playerMove.PlayerId != CurrentPlayerId && !dryRun)
            return new PlayerMoveResult(playerMove.PlayerId)
            {
                InvalidMessage = "It is not your move"
            };

        var player = playerManager.GetPlayer(playerMove.PlayerId);

        if (!playerMove.Tiles.All(t => player.Tiles.Any(pt => pt.Letter == t.Letter)))
            return new PlayerMoveResult(playerMove.PlayerId)
            {
                InvalidMessage = "Tiles have been placed that are not in your rack."
            };

        var playerMoveResult = boardManager.MakeMove(playerMove);

        if (playerMoveResult.IsValid && !dryRun)
        {
            TurnsTaken++;

            playerMoveResult.NextPlayer = SetNextPlayer();
            playerManager.AddToScore(playerMove.PlayerId, playerMoveResult.Points);

            UpdateLastMove(playerMoveResult);

            var remainingTiles = playerMove.Tiles.Where(t => t.RackPosition != -1).ToList();

            var neededTimes = 7 - remainingTiles.Count;

            var newTiles = bagManager.TakeTiles(neededTimes);
            var playerTiles = newTiles.Concat(remainingTiles).ToList();
            player.SetNewTiles(newTiles);
            player.SetTiles(playerTiles);

            if (playerTiles.Count == 0)
            {
                // The user has placed all of their tiles and the game is effectively over.
                // Note: challenges on the last move are not currently supported.
                var remainingTilePoints = playerManager.Players
                    .Sum(playerStateModel => playerStateModel.Tiles.Sum(t => boardManager.LetterValue(t.Letter ?? string.Empty)));
                playerManager.AddToScore(playerMove.PlayerId, remainingTilePoints);

                foreach (var otherPlayer in playerManager.Players.Where(p => p.PlayerId != playerMove.PlayerId))
                {
                    var otherRemainingTilePoints = otherPlayer.Tiles.Sum(t => boardManager.LetterValue(t.Letter ?? string.Empty));
                    playerManager.AddToScore(otherPlayer.PlayerId, -otherRemainingTilePoints);
                }

                var gameWinners = GetWinners();
                endGameManager.SetWinners(gameWinners.Select(p => p.PlayerId).ToList(), gameWinners[0].Score);
            }
        }

        return playerMoveResult;
    }

    public void SetLastMovedToChallengeResolved()
    {
        LastMoveType = LastMoveType.ChallengeResolved;
    }

    public Guid SetNextPlayer()
    {
        var nextPlayerId = PlayerOrderIds.NextOf(CurrentPlayerId);
        CurrentPlayerId = nextPlayerId;
        return nextPlayerId;
    }

    public void SetRandomPlayerOrder()
    {
        var playerIds = playerManager.GetPlayers()
            .Select(p => p.PlayerId).ToList();

        playerIds.Shuffle();

        SetPlayerOrder(playerIds);
    }

    public void UpdateInvitedPlayer(Guid oldPlayerId, Guid newPlayerId, string newPlayerName)
    {
        PlayerOrderIds[PlayerOrderIds.IndexOf(oldPlayerId)] = newPlayerId;

        if (CurrentPlayerId == oldPlayerId)
            CurrentPlayerId = newPlayerId;

        var player = playerManager.GetPlayer(oldPlayerId);
        player.UpdateInvitedPlayer(newPlayerId, newPlayerName);
    }

    public void InitNew()
    {
        TurnsTaken = 0;
        LastMoveType = LastMoveType.Null;
        LastMoveResult = null;
    }

    public void InitFromStateModel(PlayerMoveStateModel playerMoveStateModel)
    {
        CurrentPlayerId = playerMoveStateModel.CurrentPlayerId;
        PlayerOrderIds = playerMoveStateModel.PlayerOrderIds;
        TurnsTaken = playerMoveStateModel.TurnsTaken;
        LastMoveType = playerMoveStateModel.LastMoveType;
        LastMoveResult = playerMoveStateModel.LastMoveResult;
    }

    public void SetPlayerOrder(List<Guid> playerOrderIds)
    {
        CurrentPlayerId = playerOrderIds[0];
        PlayerOrderIds = playerOrderIds;

        foreach (var playerId in PlayerOrderIds)
        {
            var initialTiles = bagManager.TakeTiles(7);
            playerManager.SetTiles(playerId, initialTiles);
        }
    }

    public void SetCurrentPlayer(Guid playerId)
    {
        CurrentPlayerId = playerId;
    }

    public void UpdateLastMove(PlayerMoveResult playerMoveResult)
    {
        LastMoveResult = playerMoveResult;
    }

    private List<IPlayer> GetWinners()
    {
        var players = playerManager.GetPlayers()
            .OrderByDescending(p => p.Score).ToList();

        return players.Where(p => p.Score == players[0].Score).ToList();
    }
}