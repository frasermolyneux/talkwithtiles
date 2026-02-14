using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Web.Models;

public class IssueChallengeModel
{
    public IssueChallengeModel()
    {
    }

    public IssueChallengeModel(Guid gameId)
    {
        GameId = gameId;
    }

    public Guid GameId { get; set; }

    [DisplayName("Challenge Reason")] public GameChallengeReason GameChallengeReason { get; set; }

    [DataType(DataType.MultilineText)]
    [DisplayName("Challenge Text")]
    public string? ChallengeText { get; set; }
}