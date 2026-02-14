using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Contracts.Interfaces;

public interface IGameEngineFactory
{
    IGameEngine CreateNew(GamePrivacyType gamePrivacyType, GameType gameType,
        TileBagVisibilityOption tileBagVisibilityOption, bool canOverrideChallengeOutcome,
        Dictionary<GameChallengeReason, GameChallengeResult> challengeResults);

    IGameEngine CreateFromStateModel(GameStateModel gameStateModel);
}