using System;
using MX.TalkWithTiles.Repository.CloudEntities;
using MX.TalkWithTiles.Repository.Dtos;

namespace MX.TalkWithTiles.Repository.Extensions;

public static class GameInviteEntityExtensions
{
    public static GameInviteDto ToDto(this GameInviteEntity entity)
    {
        return new GameInviteDto
        {
            InviteId = Guid.Parse(entity.RowKey),
            Email = entity.PartitionKey,
            GameId = entity.GameId
        };
    }
}
