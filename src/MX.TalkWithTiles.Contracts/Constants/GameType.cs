using System.ComponentModel.DataAnnotations;

namespace MX.TalkWithTiles.Contracts.Constants;

public enum GameType
{
    [Display(Name = "Standard Board")] StandardBoard,
    [Display(Name = "Super Size Board")] SuperSizeBoard,
    [Display(Name = "Mini Board")] MiniBoard
}
