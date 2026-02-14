using System;
using Azure;
using Azure.Data.Tables;

namespace MX.TalkWithTiles.Repository.CloudEntities;

public class ContactCloudEntity : ITableEntity
{
    public ContactCloudEntity()
    {
    }

    public ContactCloudEntity(Guid userId, Guid contactId, string contactName)
    {
        PartitionKey = userId.ToString();
        RowKey = contactId.ToString();
        ContactName = contactName;
    }

    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string ContactName { get; set; } = string.Empty;
}