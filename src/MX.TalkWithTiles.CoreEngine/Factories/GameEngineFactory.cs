using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.CoreEngine.Factories;

public class GameEngineFactory(IManagerFactory managerFactory) : IGameEngineFactory
{
    public IGameEngine CreateNew(
        GamePrivacyType gamePrivacyType,
        GameType gameType,
        TileBagVisibilityOption tileBagVisibilityOption,
        bool canOverrideChallengeOutcome,
        Dictionary<GameChallengeReason, GameChallengeResult> challengeResults)
    {
        var gameEngine = new GameEngine(managerFactory);

        gameEngine.InitNew(gamePrivacyType, gameType, tileBagVisibilityOption,
            canOverrideChallengeOutcome, challengeResults);

        return gameEngine;
    }

    public IGameEngine CreateFromStateModel(GameStateModel gameStateModel)
    {
        var gameEngine = new GameEngine(managerFactory);
        gameEngine.InitFromStateModel(gameStateModel);
        return gameEngine;
    }
}
