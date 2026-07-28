namespace MX.TalkWithTiles.Contracts.Models;

public class BoardSize(int width, int height)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
}
