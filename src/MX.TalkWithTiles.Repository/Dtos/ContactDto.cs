using System;

namespace MX.TalkWithTiles.Repository.Dtos;

public class ContactDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset LastPlayed { get; set; }
}
