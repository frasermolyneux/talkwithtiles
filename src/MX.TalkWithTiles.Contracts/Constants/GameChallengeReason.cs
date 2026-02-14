using System.ComponentModel.DataAnnotations;

// ReSharper disable IdentifierTypo

namespace MX.TalkWithTiles.Contracts.Constants;

public enum GameChallengeReason
{
    [Display(Name = "That's not a word")] ThatsNotAWord,

    [Display(Name = "That's not a valid turn")]
    ThatsNotAValidTurn,

    [Display(Name = "Catchall challenge")] Catchall
}