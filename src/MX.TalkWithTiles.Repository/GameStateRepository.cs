using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
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

public class GameStateRepository(IOptions<AppDataOptions> options, ILogger<GameStateRepository> logger) : AppDataRepository(options), IGameStateRepository
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

            if (!skipTileFetch)
            {
                if (gameStateModelCloudEntity.BagStateModel.Tiles == null)
                {
                    var bagTiles = await GetTiles($"{gameId}-bag");
                    gameStateModelCloudEntity.BagStateModel.Tiles = bagTiles;
                }

                if (gameStateModelCloudEntity.BoardStateModel.Tiles == null)
                {
                    var boardTileList = await GetTiles($"{gameId}-board");

                    var boardSize = BoardSizeHelper.GetSize(gameStateModelCloudEntity.GameType);
                    var boardTiles = new Tile[boardSize.Width, boardSize.Height];

                    foreach (var tile in boardTileList) boardTiles[tile.PosX, tile.PosY] = tile;

                    gameStateModelCloudEntity.BoardStateModel.Tiles = boardTiles;
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
                BoardStateModel = gameStateModelCloudEntity.BoardStateModel,
                BagStateModel = gameStateModelCloudEntity.BagStateModel,
                EndGameStateModel = gameStateModelCloudEntity.EndGameStateModel,
                PlayersStateModel = gameStateModelCloudEntity.PlayersStateModel,
                PlayerMoveStateModel = gameStateModelCloudEntity.PlayerMoveStateModel,
                ChallengeStateModel = gameStateModelCloudEntity.ChallengeStateModel
            };
        }

        public async Task UpdateGameState(Guid gameId, GameStateModel gameStateModel)
        {
            var gameStateCloudEntity = new GameStateCloudEntity(gameId, gameStateModel);

            if (gameStateCloudEntity.BagStateModel.Tiles != null)
                await SaveTiles($"{gameStateModel.GameId}-bag", gameStateCloudEntity.BagStateModel.Tiles);

            if (gameStateCloudEntity.BoardStateModel.Tiles != null)
                await SaveTiles($"{gameStateModel.GameId}-board",
                    gameStateCloudEntity.BoardStateModel.Tiles.Cast<Tile>().ToArray());

            gameStateModel.BagStateModel.Tiles = null;
            gameStateModel.BoardStateModel.Tiles = null;

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
            if (filterModel == null) throw new NullReferenceException(nameof(filterModel));

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
                result.Add(tileEntity.Tile!);
            }

            return result;
        }

        private async Task SaveTiles(string partitionKey, IEnumerable<Tile> tiles)
        {
            try
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
            catch (Exception e)
            {
                logger.LogError(e, "Failed to save tiles for partition {PartitionKey}", partitionKey);
                throw;
            }
        }
}