using System;
using Azure;
using Azure.Data.Tables;
using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Repository.CloudEntities;

internal class GameStateIndexCloudEntity : ITableEntity
{
    public GameStateIndexCloudEntity()
    {
    }

    public GameStateIndexCloudEntity(
        Guid playerId,
        Guid gameId,
        GamePrivacyType gamePrivacyType,
        GameStateType gameStateType,
        GameType gameType)
    {
        GamePrivacyType = gamePrivacyType;
        PartitionKey = playerId.ToString();
        RowKey = gameId.ToString();
        GameStateType = gameStateType;
        GameType = gameType;
    }

    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string? GamePrivacyTypeValue { get; set; }
    public GamePrivacyType GamePrivacyType
    {
        get => Enum.TryParse<GamePrivacyType>(GamePrivacyTypeValue, out var val) ? val : default;
        set => GamePrivacyTypeValue = value.ToString();
    }

    public string? GameStateTypeValue { get; set; }
    public GameStateType GameStateType
    {
        get => Enum.TryParse<GameStateType>(GameStateTypeValue, out var val) ? val : default;
        set => GameStateTypeValue = value.ToString();
    }

    public string? GameTypeValue { get; set; }
    public GameType GameType
    {
        get => Enum.TryParse<GameType>(GameTypeValue, out var val) ? val : default;
        set => GameTypeValue = value.ToString();
    }
}