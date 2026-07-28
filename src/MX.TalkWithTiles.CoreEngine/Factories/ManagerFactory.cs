using System;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.CoreEngine.Game;
using MX.TalkWithTiles.Scrabble;

namespace MX.TalkWithTiles.CoreEngine.Factories;

public class ManagerFactory(
    ITileFactory tileFactory,
    IPlayerFactory playerFactory) : IManagerFactory
{
    public IBagManager CreateBagManager(GameType gameType)
    {
        var bagManager = new BagManager(tileFactory);
        bagManager.InitNew(gameType);
        return bagManager;
    }

    public IBagManager CreateBagManager(BagStateModel bagStateModel)
    {
        var bagManager = new BagManager(tileFactory);
        bagManager.InitFromStateModel(bagStateModel);
        return bagManager;
    }

    public IBoardManager CreateBoardManager(GameType gameType)
    {
        return gameType switch
        {
            GameType.StandardBoard or GameType.SuperSizeBoard or GameType.MiniBoard => CreateAndInitBoardManager(gameType),
            _ => throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null)
        };
    }

    public IBoardManager CreateBoardManager(GameType gameType, BoardStateModel boardStateModel)
    {
        var boardManager = new ScrabbleBoardManager(tileFactory);
        boardManager.InitFromStateModel(gameType, boardStateModel);
        return boardManager;
    }

    public IPlayerMoveManager CreatePlayerMoveManager(IBagManager bagManager, IBoardManager boardManager,
        IPlayerManager playerManager, IEndGameManager endGameManager)
    {
        var playerMoveManager = new PlayerMoveManager(bagManager, boardManager, playerManager, endGameManager);
        playerMoveManager.InitNew();
        return playerMoveManager;
    }

    public IPlayerMoveManager CreatePlayerMoveManager(PlayerMoveStateModel playerMoveStateModel,
        IBagManager bagManager,
        IBoardManager boardManager, IPlayerManager playerManager, IEndGameManager endGameManager)
    {
        var playerMoveManager = new PlayerMoveManager(bagManager, boardManager, playerManager, endGameManager);
        playerMoveManager.InitFromStateModel(playerMoveStateModel);
        return playerMoveManager;
    }

    public IChallengeManager CreateChallengeManager(bool canOverrideChallengeOutcome,
        Dictionary<GameChallengeReason, GameChallengeResult> challengeResults, IPlayerMoveManager playerMoveManager)
    {
        var challengerManager = new ChallengeManager(playerMoveManager);
        challengerManager.InitNew(canOverrideChallengeOutcome, challengeResults);
        return challengerManager;
    }

    public IChallengeManager CreateChallengeManager(ChallengeStateModel challengeStateModel,
        IPlayerMoveManager playerMoveManager)
    {
        var challengerManager = new ChallengeManager(playerMoveManager);
        challengerManager.InitFromStateModel(challengeStateModel);
        return challengerManager;
    }

    public IEndGameManager CreateEndGameManager()
    {
        var endGameManager = new EndGameManager();
        endGameManager.InitNew();
        return endGameManager;
    }

    public IEndGameManager CreateEndGameManager(EndGameStateModel endGameStateModel)
    {
        var endGameManager = new EndGameManager();
        endGameManager.InitFromStateModel(endGameStateModel);
        return endGameManager;
    }

    public IPlayerManager CreatePlayerManager()
    {
        var playerManager = new PlayerManager(playerFactory);
        playerManager.InitNew();
        return playerManager;
    }

    public IPlayerManager CreatePlayerManager(PlayersStateModel playersStateModel)
    {
        var playerManager = new PlayerManager(playerFactory);
        playerManager.InitFromStateModel(playersStateModel);
        return playerManager;
    }

    private IBoardManager CreateAndInitBoardManager(GameType gameType)
    {
        var boardManager = new ScrabbleBoardManager(tileFactory);
        boardManager.InitNew(gameType);
        return boardManager;
    }
}
