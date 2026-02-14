using System.ComponentModel.DataAnnotations;

namespace MX.TalkWithTiles.Contracts.Constants;

public enum GameChallengeResult
{
    [Display(Name = "Retry player move")] RetryPlayerMove,

    [Display(Name = "Lose points and proceed to next player")]
    LosePointsAndProceedToNextPlayer,
    [Display(Name = "Nothing")] Nothing
}