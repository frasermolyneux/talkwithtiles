using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.CoreEngine.Game;

public class ChallengeManager(IPlayerMoveManager playerMoveManager) : IChallengeManager
{
    public bool CanOverrideChallengeOutcome { get; set; }
    public Dictionary<GameChallengeReason, GameChallengeResult> ChallengeResults { get; private set; } = [];
    private Guid ChallengedPlayerId { get; set; }
    private Guid ChallengerPlayerId { get; set; }
    private GameChallengeReason? ChallengeReason { get; set; }
    private string? ChallengeText { get; set; }

    public PlayerChallengeResult? PlayerChallengeResult { get; set; }

    public ChallengeStateModel ChallengeStateModel =>
        new()
        {
            CanOverrideChallengeOutcome = CanOverrideChallengeOutcome,
            ChallengeResults = ChallengeResults,
            ChallengedPlayerId = ChallengedPlayerId,
            ChallengerPlayerId = ChallengerPlayerId,
            ChallengeReason = ChallengeReason,
            ChallengeText = ChallengeText,
            PlayerChallengeResult = PlayerChallengeResult
        };

    public void IssuePlayerChallenge(Guid playerId, GameChallengeReason gameChallengeReason, string challengeText)
    {
        ChallengedPlayerId = playerMoveManager.LastMoveResult!.PlayerId;
        ChallengerPlayerId = playerId;
        ChallengeReason = gameChallengeReason;
        ChallengeText = challengeText;
    }

    public void ResolveChallenge(bool accepted, GameChallengeResult? overrideChallengeResult)
    {
        if (accepted && CanOverrideChallengeOutcome && overrideChallengeResult != null)
        {
            switch (overrideChallengeResult)
            {
                case GameChallengeResult.RetryPlayerMove:
                    RetryPlayerMove();
                    break;
                case GameChallengeResult.LosePointsAndProceedToNextPlayer:
                    LosePointsAndProceedToNextPlayer();
                    break;
                case GameChallengeResult.Nothing:
                    ResetChallengeManager(GameChallengeResult.Nothing);
                    break;
            }
        }
        else if (accepted && ChallengeReason != null)
        {
            var challengeResult = ChallengeResults[(GameChallengeReason)ChallengeReason];

            switch (challengeResult)
            {
                case GameChallengeResult.RetryPlayerMove:
                    RetryPlayerMove();
                    break;
                case GameChallengeResult.LosePointsAndProceedToNextPlayer:
                    LosePointsAndProceedToNextPlayer();
                    break;
                case GameChallengeResult.Nothing:
                    ResetChallengeManager(GameChallengeResult.Nothing);
                    break;
            }
        }
        else
        {
            ResetChallengeManager(GameChallengeResult.Nothing);
        }
    }

    public void InitNew(bool canOverrideChallengeOutcome,
        Dictionary<GameChallengeReason, GameChallengeResult> challengeResults)
    {
        CanOverrideChallengeOutcome = canOverrideChallengeOutcome;
        ChallengeResults = challengeResults;
        ChallengedPlayerId = Guid.Empty;
        ChallengerPlayerId = Guid.Empty;
        ChallengeReason = null;
        ChallengeText = string.Empty;
        PlayerChallengeResult = null;
    }

    public void InitFromStateModel(ChallengeStateModel challengeStateModel)
    {
        CanOverrideChallengeOutcome = challengeStateModel.CanOverrideChallengeOutcome;
        ChallengeResults = challengeStateModel.ChallengeResults;
        ChallengedPlayerId = challengeStateModel.ChallengedPlayerId;
        ChallengerPlayerId = challengeStateModel.ChallengerPlayerId;
        ChallengeReason = challengeStateModel.ChallengeReason;
        ChallengeText = challengeStateModel.ChallengeText;
        PlayerChallengeResult = challengeStateModel.PlayerChallengeResult;
    }

    private void SetPlayerChallengeResult(GameChallengeResult gameChallengeResult)
    {
        PlayerChallengeResult = new PlayerChallengeResult
        {
            ChallengedPlayerId = ChallengedPlayerId,
            ChallengerPlayerId = ChallengerPlayerId,
            // ReSharper disable once PossibleInvalidOperationException
            GameChallengeReason = (GameChallengeReason)ChallengeReason!,
            GameChallengeResult = gameChallengeResult
        };
    }

    private void RetryPlayerMove()
    {
        playerMoveManager.UndoLastTurn(ChallengedPlayerId);
        ResetChallengeManager(GameChallengeResult.RetryPlayerMove);
    }

    private void LosePointsAndProceedToNextPlayer()
    {
        playerMoveManager.UndoLastTurn(ChallengedPlayerId);
        playerMoveManager.SetNextPlayer();

        ResetChallengeManager(GameChallengeResult.LosePointsAndProceedToNextPlayer);
    }

    private void ResetChallengeManager(GameChallengeResult gameChallengeResult)
    {
        SetPlayerChallengeResult(gameChallengeResult);

        ChallengedPlayerId = Guid.Empty;
        ChallengerPlayerId = Guid.Empty;
        ChallengeReason = null;
        ChallengeText = string.Empty;

        playerMoveManager.SetLastMovedToChallengeResolved();
    }
}