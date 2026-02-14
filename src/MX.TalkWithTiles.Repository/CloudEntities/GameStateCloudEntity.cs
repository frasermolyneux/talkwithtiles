using System;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Repository.CloudEntities;

public class GameStateCloudEntity : ITableEntity
{
    public GameStateCloudEntity()
    {
    }

    public GameStateCloudEntity(Guid gameId, GameStateModel gameStateModel)
    {
        PartitionKey = "scrabble";
        RowKey = gameId.ToString();

        GamePrivacyType = gameStateModel.GamePrivacyType;
        BoardStateModel = gameStateModel.BoardStateModel;
        BagStateModel = gameStateModel.BagStateModel;
        EndGameStateModel = gameStateModel.EndGameStateModel;
        PlayersStateModel = gameStateModel.PlayersStateModel;
        ChallengeStateModel = gameStateModel.ChallengeStateModel;
        GameType = gameStateModel.GameType;
        PlayerMoveStateModel = gameStateModel.PlayerMoveStateModel;
        TileBagVisibilityOption = gameStateModel.TileBagVisibilityOption;
    }

    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // Enum properties stored as strings
    public string? GamePrivacyTypeValue { get; set; }
    public GamePrivacyType GamePrivacyType
    {
        get => Enum.TryParse<GamePrivacyType>(GamePrivacyTypeValue, out var val) ? val : default;
        set => GamePrivacyTypeValue = value.ToString();
    }

    public string? GameTypeValue { get; set; }
    public GameType GameType
    {
        get => Enum.TryParse<GameType>(GameTypeValue, out var val) ? val : default;
        set => GameTypeValue = value.ToString();
    }

    public string? TileBagVisibilityOptionValue { get; set; }
    public TileBagVisibilityOption TileBagVisibilityOption
    {
        get => Enum.TryParse<TileBagVisibilityOption>(TileBagVisibilityOptionValue, out var val) ? val : default;
        set => TileBagVisibilityOptionValue = value.ToString();
    }

    // JSON-serialized complex properties
    public string? BoardStateModelJson { get; set; }
    public BoardStateModel? BoardStateModel
    {
        get => string.IsNullOrEmpty(BoardStateModelJson) ? null : JsonSerializer.Deserialize<BoardStateModel>(BoardStateModelJson);
        set => BoardStateModelJson = value == null ? null : JsonSerializer.Serialize(value);
    }

    public string? BagStateModelJson { get; set; }
    public BagStateModel? BagStateModel
    {
        get => string.IsNullOrEmpty(BagStateModelJson) ? null : JsonSerializer.Deserialize<BagStateModel>(BagStateModelJson);
        set => BagStateModelJson = value == null ? null : JsonSerializer.Serialize(value);
    }

    public string? PlayersStateModelJson { get; set; }
    public PlayersStateModel? PlayersStateModel
    {
        get => string.IsNullOrEmpty(PlayersStateModelJson) ? null : JsonSerializer.Deserialize<PlayersStateModel>(PlayersStateModelJson);
        set => PlayersStateModelJson = value == null ? null : JsonSerializer.Serialize(value);
    }

    public string? EndGameStateModelJson { get; set; }
    public EndGameStateModel? EndGameStateModel
    {
        get => string.IsNullOrEmpty(EndGameStateModelJson) ? null : JsonSerializer.Deserialize<EndGameStateModel>(EndGameStateModelJson);
        set => EndGameStateModelJson = value == null ? null : JsonSerializer.Serialize(value);
    }

    public string? PlayerMoveStateModelJson { get; set; }
    public PlayerMoveStateModel? PlayerMoveStateModel
    {
        get => string.IsNullOrEmpty(PlayerMoveStateModelJson) ? null : JsonSerializer.Deserialize<PlayerMoveStateModel>(PlayerMoveStateModelJson);
        set => PlayerMoveStateModelJson = value == null ? null : JsonSerializer.Serialize(value);
    }

    public string? ChallengeStateModelJson { get; set; }
    public ChallengeStateModel? ChallengeStateModel
    {
        get => string.IsNullOrEmpty(ChallengeStateModelJson) ? null : JsonSerializer.Deserialize<ChallengeStateModel>(ChallengeStateModelJson);
        set => ChallengeStateModelJson = value == null ? null : JsonSerializer.Serialize(value);
    }
}