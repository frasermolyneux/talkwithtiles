using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Web.Models;

public class ResolveChallengeModel
{
    [Required]
    public Guid? Id { get; set; }
    [BindRequired]
    public bool Accept { get; set; }
    [Required]
    public GameChallengeResult? GameChallengeResultOverride { get; set; }
    [BindRequired]
    public bool AllowOverride { get; set; }
}