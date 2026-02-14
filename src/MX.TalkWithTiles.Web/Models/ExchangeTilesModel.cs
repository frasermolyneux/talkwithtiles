namespace MX.TalkWithTiles.Web.Models;

public class ExchangeTilesModel
{
    public Guid Id { get; set; }
    public List<ExchangeTile> ExchangeTiles { get; set; } = [];
}