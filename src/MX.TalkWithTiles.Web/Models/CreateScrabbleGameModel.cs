using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Repository.Dtos;

namespace MX.TalkWithTiles.Web.Models;

public class CreateScrabbleGameModel
{
    public List<CreatePlayerModel> PlayerModels { get; set; } = [];

    [DisplayName("Should this game be publicly listed?")]
    public bool PublicGame { get; set; }

    [DisplayName("What Scrabble board do you want to use?")]
    public GameType GameType { get; set; }

    [DisplayName("When should we display remaining tiles?")]
    public TileBagVisibilityOption TileBagVisibilityOption { get; set; }

    [DisplayName("The default challenge outcome can be overridden at game time")]
    public bool CanOverrideChallengeOutcome { get; set; } = true;

    [DisplayName("\"That's not a word!\"")]
    public GameChallengeResult ThatsNotAWordGameChallengeResult { get; set; } = GameChallengeResult.RetryPlayerMove;

    [DisplayName("\"That's not a valid turn!\"")]
    public GameChallengeResult ThatsNotAValidTurnGameChallengeResult { get; set; } =
        GameChallengeResult.RetryPlayerMove;

    [DisplayName("Catchall challenge")]
    public GameChallengeResult CatchallGameChallengeResult { get; set; } = GameChallengeResult.RetryPlayerMove;

    public List<ContactDto>? ContactDtos { get; set; }
}

public class CreatePlayerModel
{
    [Required(ErrorMessage = "This field is required.")]
    [DisplayName("Opponents username or email")]
    public string Identifier { get; set; } = string.Empty;
}