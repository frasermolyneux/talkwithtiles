using System.ComponentModel.DataAnnotations;

namespace MX.TalkWithTiles.Web.Models;

public class Feedback
{
    [Required]
    [DataType(DataType.MultilineText)]
    public required string FeedbackText { get; set; }
}
