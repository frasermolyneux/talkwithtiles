using System;

namespace MX.TalkWithTiles.Repository.Dtos;

public class GameInviteDto
{
    public Guid InviteId { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid GameId { get; set; }
}