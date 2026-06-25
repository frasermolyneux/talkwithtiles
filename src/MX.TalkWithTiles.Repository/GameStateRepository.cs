using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.CoreEngine.Helpers;
using MX.TalkWithTiles.Repository.CloudEntities;
using MX.TalkWithTiles.Repository.Config;
using MX.TalkWithTiles.Repository.Extensions;
using MX.TalkWithTiles.Repository.Interfaces;
using MX.TalkWithTiles.Repository.Models;

namespace MX.TalkWithTiles.Repository;

public class GameStateRepository(IOptions<AppDataOptions> options) : AppDataRepository(options), IGameStateRepository
{
    public async Task<GameStateModel?> GetGameState(Guid gameId, bool skipTileFetch = false)
    {
        GameStateCloudEntity gameStateModelCloudEntity;
        try
        {
            var response = await GameStateTable.GetEntityAsync<GameStateCloudEntity>("scrabble", gameId.ToString());
            gameStateModelCloudEntity = response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }

        // Read computed properties once — each getter access deserializes
        // from JSON, so repeated access creates new throwaway objects.
        var bagStateModel = gameStateModelCloudEntity.BagStateModel ?? new BagStateModel();
        var boardStateModel = gameStateModelCloudEntity.BoardStateModel ?? new BoardStateModel();

        if (!skipTileFetch)
        {
            if (bagStateModel.Tiles == null)
            {
                var bagTiles = await GetTiles($"{gameId}-bag");
                bagStateModel.Tiles = bagTiles;
            }

            if (boardStateModel.Tiles == null)
            {
                var boardTileList = await GetTiles($"{gameId}-board");

                var boardSize = BoardSizeHelper.GetSize(gameStateModelCloudEntity.GameType);
                var boardTiles = new Tile[boardSize.Width, boardSize.Height];

                foreach (var tile in boardTileList) boardTiles[tile.PosX, tile.PosY] = tile;

                boardStateModel.Tiles = boardTiles;
            }
        }

        return new GameStateModel
        {
            GameId = Guid.Parse(gameStateModelCloudEntity.RowKey),
            GameEtag = gameStateModelCloudEntity.Timestamp?.ToUnixTimeMilliseconds().ToString(),
            LastUpdated = gameStateModelCloudEntity.Timestamp ?? DateTimeOffset.MinValue,
            GameType = gameStateModelCloudEntity.GameType,
            TileBagVisibilityOption = gameStateModelCloudEntity.TileBagVisibilityOption,
            GamePrivacyType = gameStateModelCloudEntity.GamePrivacyType,
            BoardStateModel = boardStateModel,
            BagStateModel = bagStateModel,
            EndGameStateModel = gameStateModelCloudEntity.EndGameStateModel ?? new EndGameStateModel(),
            PlayersStateModel = gameStateModelCloudEntity.PlayersStateModel ?? new PlayersStateModel(),
            PlayerMoveStateModel = gameStateModelCloudEntity.PlayerMoveStateModel ?? new PlayerMoveStateModel(),
            ChallengeStateModel = gameStateModelCloudEntity.ChallengeStateModel ?? new ChallengeStateModel()
        };
    }

    public async Task UpdateGameState(Guid gameId, GameStateModel gameStateModel)
    {
        // Save tiles separately before constructing cloud entity to avoid
        // serializing Tile[,] which System.Text.Json does not support.
        if (gameStateModel.BagStateModel.Tiles != null)
            await SaveTiles($"{gameStateModel.GameId}-bag", gameStateModel.BagStateModel.Tiles);

        if (gameStateModel.BoardStateModel.Tiles != null)
            await SaveTiles($"{gameStateModel.GameId}-board",
                gameStateModel.BoardStateModel.Tiles.Cast<Tile>().ToArray());

        gameStateModel.BagStateModel.Tiles = null;
        gameStateModel.BoardStateModel.Tiles = null;

        var gameStateCloudEntity = new GameStateCloudEntity(gameId, gameStateModel);

        await GameStateTable.UpsertEntityAsync(gameStateCloudEntity, TableUpdateMode.Merge);

        foreach (var scrabblePlayerStateModel in gameStateModel.PlayersStateModel.Players)
        {
            var scrabbleGameStateIndexCloudEntity =
                new GameStateIndexCloudEntity(
                    scrabblePlayerStateModel.PlayerId,
                    gameStateModel.GameId,
                    gameStateModel.GamePrivacyType,
                    gameStateModel.EndGameStateModel.GameStateType,
                    gameStateModel.GameType);

            await GameStateIndexTable.UpsertEntityAsync(scrabbleGameStateIndexCloudEntity, TableUpdateMode.Merge);
        }
    }

    public async Task<List<GameStateModel>> GetGameStates(GameStateFilterModel filterModel)
    {
        if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

        var indexResults = new List<GameStateIndexCloudEntity>();
        var filter = GameStateIndexCloudEntityExtensions.BuildFilter(filterModel);

        await foreach (var entity in GameStateIndexTable.QueryAsync<GameStateIndexCloudEntity>(filter))
        {
            indexResults.Add(entity);
        }

        var gameIds = indexResults.Select(i => i.RowKey).Distinct().ToList();

        var results = new List<GameStateModel>();
        foreach (var gameId in gameIds)
        {
            var gameStateModel = await GetGameState(Guid.Parse(gameId), filterModel.SkipTileFetch);

            if (gameStateModel != null)
                results.Add(gameStateModel);
        }

        return results.ToList();
    }

    public async Task DeleteGameStateIndex(Guid id, Guid playerId)
    {
        await GameStateIndexTable.DeleteEntityAsync(playerId.ToString(), id.ToString(), ETag.All);
    }

    private async Task<List<Tile>> GetTiles(string partitionKey)
    {
        var result = new List<Tile>();

        await foreach (var tileEntity in TilesTable.QueryAsync<TileCloudEntity>(x => x.PartitionKey == partitionKey))
        {
            if (tileEntity.Tile is { } tile)
                result.Add(tile);
        }

        return result;
    }

    private async Task SaveTiles(string partitionKey, IEnumerable<Tile> tiles)
    {
        // Delete existing tiles in batches
        var entitiesToDelete = new List<TableEntity>();
        await foreach (var entity in TilesTable.QueryAsync<TableEntity>(x => x.PartitionKey == partitionKey, select: new[] { "PartitionKey", "RowKey" }))
        {
            entitiesToDelete.Add(entity);
        }

        var deleteBatches = entitiesToDelete.Batch(100);
        foreach (var deleteBatch in deleteBatches)
        {
            var batch = new List<TableTransactionAction>();
            foreach (var entity in deleteBatch)
            {
                batch.Add(new TableTransactionAction(TableTransactionActionType.Delete, entity, ETag.All));
            }
            await TilesTable.SubmitTransactionAsync(batch);
        }

        // Insert new tiles in batches
        var bagTileBatches = tiles.Batch(100);
        foreach (var bagTileBatch in bagTileBatches)
        {
            var batch = new List<TableTransactionAction>();

            foreach (var tile in bagTileBatch)
            {
                var tileEntity = new TileCloudEntity(tile)
                {
                    PartitionKey = partitionKey,
                    RowKey = Guid.NewGuid().ToString()
                };

                batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, tileEntity));
            }

            await TilesTable.SubmitTransactionAsync(batch);
        }
    }
}