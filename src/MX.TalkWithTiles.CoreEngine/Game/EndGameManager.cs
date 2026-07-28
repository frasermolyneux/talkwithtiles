using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.CoreEngine.Game;

public class EndGameManager : IEndGameManager
{
    private GameStateType GameStateType { get; set; }
    private Dictionary<Guid, int> SkippedTurns { get; set; } = [];
    private List<Guid> Winners { get; set; } = [];
    private int WinnerPoints { get; set; }
    private Guid AbandoningPlayerId { get; set; }

    public EndGameStateModel EndGameStateModel =>
        new()
        {
            GameStateType = GameStateType,
            SkippedTurns = SkippedTurns,
            Winners = Winners,
            WinnerPoints = WinnerPoints,
            AbandoningPlayerId = AbandoningPlayerId
        };

    public void SetWinners(List<Guid> playerIds, int winningScore)
    {
        GameStateType = GameStateType.Completed;
        Winners = playerIds;
        WinnerPoints = winningScore;
    }

    public void AbandonGame(Guid playerId)
    {
        GameStateType = GameStateType.Abandoned;
        AbandoningPlayerId = playerId;
    }

    public void InitNew()
    {
        GameStateType = GameStateType.InProgress;
        SkippedTurns = [];
    }

    public void InitFromStateModel(EndGameStateModel endGameStateModel)
    {
        GameStateType = endGameStateModel.GameStateType;
        SkippedTurns = endGameStateModel.SkippedTurns;
        Winners = endGameStateModel.Winners;
        WinnerPoints = endGameStateModel.WinnerPoints;
        AbandoningPlayerId = endGameStateModel.AbandoningPlayerId;
    }
}
