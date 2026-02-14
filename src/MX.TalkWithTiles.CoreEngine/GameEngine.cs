using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.CoreEngine;

public class GameEngine(IManagerFactory managerFactory) : IGameEngine
{
    private IBoardManager BoardManager { get; set; } = null!;
    private IBagManager BagManager { get; set; } = null!;
    private IPlayerManager PlayerManager { get; set; } = null!;
    private IEndGameManager EndGameManager { get; set; } = null!;
    private IChallengeManager ChallengeManager { get; set; } = null!;
    private IPlayerMoveManager PlayerMoveManager { get; set; } = null!;

    private Guid GameId { get; set; }
    private GamePrivacyType GamePrivacyType { get; set; }
    public GameType GameType { get; set; }
    public TileBagVisibilityOption TileBagVisibilityOption { get; set; }

    public GameStateModel GameStateModel =>
        new()
        {
            GameId = GameId,
            GamePrivacyType = GamePrivacyType,
            GameType = GameType,
            TileBagVisibilityOption = TileBagVisibilityOption,

            BoardStateModel = BoardManager.BoardStateModel,
            BagStateModel = BagManager.BagStateModel,
            PlayersStateModel = PlayerManager.PlayersStateModel,
            EndGameStateModel = EndGameManager.EndGameStateModel,
            PlayerMoveStateModel = PlayerMoveManager.PlayerMoveStateModel,
            ChallengeStateModel = ChallengeManager.ChallengeStateModel
        };

    public void AddPlayer(Guid playerId, string? playerName)
    {
        PlayerManager.AddPlayer(playerId, playerName);
    }

    public void SetRandomPlayerOrder()
    {
        PlayerMoveManager.SetRandomPlayerOrder();
    }

    public PlayerMoveResult MakeMove(PlayerMove playerMove, bool dryRun)
    {
        return PlayerMoveManager.MakeMove(playerMove, dryRun);
    }

    public void SkipMove(Guid playerId)
    {
        PlayerMoveManager.SkipTurn(playerId);
    }

    public void ExchangeTiles(Guid playerId, IEnumerable<Guid> tileIds)
    {
        PlayerMoveManager.ExchangeTiles(playerId, tileIds);
    }

    public void AbandonGame(Guid playerId)
    {
        EndGameManager.AbandonGame(playerId);
    }

    public void UpdateInvitedPlayer(Guid oldPlayerId, Guid newPlayerId, string newPlayerName)
    {
        PlayerMoveManager.UpdateInvitedPlayer(oldPlayerId, newPlayerId, newPlayerName);
    }

    public void IssuePlayerChallenge(Guid playerId, GameChallengeReason gameChallengeReason, string challengeText)
    {
        ChallengeManager.IssuePlayerChallenge(playerId, gameChallengeReason, challengeText);
    }

    public void ResolveChallenge(bool accepted, GameChallengeResult? overrideChallengeResult)
    {
        ChallengeManager.ResolveChallenge(accepted, overrideChallengeResult);
    }

    public void UndoLastTurn(Guid playerId)
    {
        PlayerMoveManager.UndoLastTurn(playerId);
    }

    public void InitNew(
        GamePrivacyType gamePrivacyType,
        GameType gameType,
        TileBagVisibilityOption tileBagVisibilityOption,
        bool canOverrideChallengeOutcome,
        Dictionary<GameChallengeReason, GameChallengeResult> challengeResults)
    {
        GameId = Guid.NewGuid();
        GamePrivacyType = gamePrivacyType;
        GameType = gameType;
        TileBagVisibilityOption = tileBagVisibilityOption;

        BoardManager = managerFactory.CreateBoardManager(GameType);
        BagManager = managerFactory.CreateBagManager(GameType);
        PlayerManager = managerFactory.CreatePlayerManager();
        EndGameManager = managerFactory.CreateEndGameManager();
        PlayerMoveManager = managerFactory.CreatePlayerMoveManager(
            BagManager, BoardManager, PlayerManager, EndGameManager);
        ChallengeManager = managerFactory.CreateChallengeManager(
            canOverrideChallengeOutcome, challengeResults, PlayerMoveManager);
    }

    public void InitFromStateModel(GameStateModel gameStateModel)
    {
        GameId = gameStateModel.GameId;
        GamePrivacyType = gameStateModel.GamePrivacyType;
        GameType = gameStateModel.GameType;
        TileBagVisibilityOption = gameStateModel.TileBagVisibilityOption;

        BoardManager = managerFactory.CreateBoardManager(GameType, gameStateModel.BoardStateModel);
        BagManager = managerFactory.CreateBagManager(gameStateModel.BagStateModel);
        PlayerManager = managerFactory.CreatePlayerManager(gameStateModel.PlayersStateModel);
        EndGameManager = managerFactory.CreateEndGameManager(gameStateModel.EndGameStateModel);
        PlayerMoveManager = managerFactory.CreatePlayerMoveManager(
            gameStateModel.PlayerMoveStateModel, BagManager, BoardManager, PlayerManager, EndGameManager);
        ChallengeManager = managerFactory.CreateChallengeManager(
            gameStateModel.ChallengeStateModel, PlayerMoveManager);
    }
}