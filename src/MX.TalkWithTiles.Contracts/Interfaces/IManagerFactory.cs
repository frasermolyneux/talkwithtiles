using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IManagerFactory
{
    IBagManager CreateBagManager(GameType gameType);
    IBagManager CreateBagManager(BagStateModel bagStateModel);

    IBoardManager CreateBoardManager(GameType gameType);
    IBoardManager CreateBoardManager(GameType gameType, BoardStateModel boardStateModel);

    IPlayerMoveManager CreatePlayerMoveManager(IBagManager bagManager, IBoardManager boardManager,
        IPlayerManager playerManager, IEndGameManager endGameManager);

    IPlayerMoveManager CreatePlayerMoveManager(PlayerMoveStateModel playerMoveStateModel, IBagManager bagManager,
        IBoardManager boardManager, IPlayerManager playerManager, IEndGameManager endGameManager);

    IChallengeManager CreateChallengeManager(bool canOverrideChallengeOutcome,
        Dictionary<GameChallengeReason, GameChallengeResult> challengeResults,
        IPlayerMoveManager playerMoveManager);

    IChallengeManager CreateChallengeManager(ChallengeStateModel challengeStateModel,
        IPlayerMoveManager playerMoveManager);

    IEndGameManager CreateEndGameManager();
    IEndGameManager CreateEndGameManager(EndGameStateModel endGameStateModel);

    IPlayerManager CreatePlayerManager();

    IPlayerManager CreatePlayerManager(PlayersStateModel playersStateModel);
}