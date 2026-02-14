using System.ComponentModel.DataAnnotations;
using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Web.Models;

public class ResolveChallengeModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public bool Accept { get; set; }
    [Required]
    public GameChallengeResult GameChallengeResultOverride { get; set; }
    [Required]
    public bool AllowOverride { get; set; }
}