namespace MX.TalkWithTiles.Web.Models;

public class Alert
{
    public Alert() { }

    public Alert(string message, string type)
    {
        Message = message;
        Type = type;
    }

    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
