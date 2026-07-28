using System.ComponentModel.DataAnnotations;

namespace MX.TalkWithTiles.Contracts.Constants;

public enum TileBagVisibilityOption
{
    [Display(Name = "Always show remaining tiles")]
    ShowRemainingTiles,

    [Display(Name = "Do not show remaining tiles")]
    DoNotShowRemainingTiles,

    [Display(Name = "Show remaining tiles after first turns")]
    ShowAfterFirstTurns,

    [Display(Name = "Show remaining tiles near end game")]
    ShowNearEndGame,

    [Display(Name = "Only show the count of remaining tiles")]
    OnlyShowRemainingCount
}
