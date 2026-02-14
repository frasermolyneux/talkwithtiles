using System.ComponentModel.DataAnnotations;

namespace MX.TalkWithTiles.Web.Models;

public class ExchangeTilesModel
{
    [Required]
    public Guid Id { get; set; }
    public List<ExchangeTile> ExchangeTiles { get; set; } = [];
}