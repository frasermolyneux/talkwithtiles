using System;
using System.Runtime.Serialization;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using MX.TalkWithTiles.Contracts.Models;

namespace MX.TalkWithTiles.Repository.CloudEntities;

public class TileCloudEntity : ITableEntity
{
    public TileCloudEntity()
    {
    }

    public TileCloudEntity(Tile tile)
    {
        Tile = tile;
    }

    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string? TileJson { get; set; }

    [IgnoreDataMember]
    public Tile? Tile
    {
        get => string.IsNullOrEmpty(TileJson) ? null : JsonSerializer.Deserialize<Tile>(TileJson);
        set => TileJson = value == null ? null : JsonSerializer.Serialize(value);
    }
}