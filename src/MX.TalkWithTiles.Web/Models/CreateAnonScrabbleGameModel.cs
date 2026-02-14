using System.ComponentModel.DataAnnotations;

namespace MX.TalkWithTiles.Web.Models;

public class CreateAnonScrabbleGameModel
{
    public List<CreateAnonPlayerModel> PlayerModels { get; set; } = [];
}

public class CreateAnonPlayerModel
{
    [Required(ErrorMessage = "This field is required.")]
    public required string PlayerName { get; set; }
}