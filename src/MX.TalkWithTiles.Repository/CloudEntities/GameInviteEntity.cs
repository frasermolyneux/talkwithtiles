using System;
using Azure;
using Azure.Data.Tables;

namespace MX.TalkWithTiles.Repository.CloudEntities;

public class GameInviteEntity : ITableEntity
{
    public GameInviteEntity()
    {
    }

    public GameInviteEntity(Guid inviteId, string emailAddress, Guid gameId)
    {
        PartitionKey = emailAddress;
        RowKey = inviteId.ToString();

        GameId = gameId;
    }

    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public Guid GameId { get; set; }
}