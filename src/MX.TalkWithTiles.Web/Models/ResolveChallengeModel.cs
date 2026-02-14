using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Web.Models;

public class ResolveChallengeModel
{
    public Guid Id { get; set; }
    public bool Accept { get; set; }
    public GameChallengeResult GameChallengeResultOverride { get; set; }
    public bool AllowOverride { get; set; }
}