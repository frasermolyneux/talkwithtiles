using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.Repository.Dtos;
using MX.TalkWithTiles.Repository.Interfaces;
using MX.TalkWithTiles.Repository.Models;
using Npgsql;
using NpgsqlTypes;

namespace MX.TalkWithTiles.Repository;

public sealed class GameStateRepository(NpgsqlDataSource dataSource) : IGameStateRepository
{
    public async Task<GameStateModel?> GetGameState(Guid gameId, bool skipTileFetch = false)
    {
        // Tiles are part of the same snapshot, including when a caller only needs metadata.
        await using var command = dataSource.CreateCommand(
            "SELECT state::text, version, updated_at FROM games WHERE id = $1");
        command.Parameters.AddWithValue(gameId);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadState(reader) : null;
    }

    public Task UpdateGameState(Guid gameId, GameStateModel gameStateModel) =>
        SaveGame(gameId, gameStateModel, []);

    public async Task CreateGame(GameStateModel gameStateModel, IReadOnlyList<GameInviteDto> invites)
    {
        ArgumentNullException.ThrowIfNull(gameStateModel);
        if (gameStateModel.GameEtag != null || invites.Any(i => i.GameId != gameStateModel.GameId ||
            !gameStateModel.PlayersStateModel.Players.Any(p => p.PlayerId == i.InviteId)))
        {
            throw new ArgumentException("Invites must belong to this new game.", nameof(invites));
        }

        await SaveGame(gameStateModel.GameId, gameStateModel, invites);
    }

    private async Task SaveGame(Guid gameId, GameStateModel state, IReadOnlyList<GameInviteDto> invites)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (gameId == Guid.Empty || gameId != state.GameId)
        {
            throw new ArgumentException("Game identity does not match the snapshot.", nameof(gameId));
        }

        var json = JsonSerializer.Serialize(state, GameJson.Options);
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT state = $2, version, updated_at FROM games WHERE id = $1 FOR UPDATE", connection, transaction);
        command.Parameters.AddWithValue(gameId);
        command.Parameters.AddWithValue(NpgsqlDbType.Jsonb, json);
        long version = 0;
        DateTimeOffset updatedAt = default;
        bool unchanged = false;
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                unchanged = reader.GetBoolean(0);
                version = reader.GetInt64(1);
                updatedAt = reader.GetFieldValue<DateTimeOffset>(2);
            }
        }

        if (version != 0 && unchanged)
        {
            await transaction.CommitAsync();
            state.GameEtag = version.ToString(CultureInfo.InvariantCulture);
            state.LastUpdated = updatedAt;
            return;
        }

        if ((version != 0 && state.GameEtag != version.ToString(CultureInfo.InvariantCulture)) ||
            (version == 0 && state.GameEtag != null))
        {
            throw new GameConcurrencyException();
        }

        command.Parameters.Clear();
        command.CommandText = """
            INSERT INTO games (id, state, privacy) VALUES ($1, $2, $3)
            ON CONFLICT (id) DO UPDATE SET state = EXCLUDED.state, privacy = EXCLUDED.privacy,
                version = games.version + 1, updated_at = clock_timestamp()
            RETURNING version, updated_at
            """;
        command.Parameters.AddWithValue(gameId);
        command.Parameters.AddWithValue(NpgsqlDbType.Jsonb, json);
        command.Parameters.AddWithValue((int)state.GamePrivacyType);
        // Concurrent first saves have no existing row to lock. Never let an upsert overwrite one.
        if (version == 0)
        {
            command.CommandText = """
                INSERT INTO games (id, state, privacy) VALUES ($1, $2, $3)
                ON CONFLICT (id) DO NOTHING RETURNING version, updated_at
                """;
        }

        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                throw new GameConcurrencyException();
            }

            version = reader.GetInt64(0);
            updatedAt = reader.GetFieldValue<DateTimeOffset>(1);
        }

        var playerIds = state.PlayersStateModel.Players.Select(p => p.PlayerId).ToArray();
        command.Parameters.Clear();
        command.Parameters.AddWithValue(gameId);
        command.Parameters.AddWithValue(playerIds);
        foreach (var sql in new[]
        {
            "DELETE FROM game_players WHERE game_id = $1 AND NOT (player_id = ANY($2))",
            "DELETE FROM invites WHERE game_id = $1 AND NOT (invite_id = ANY($2))",
            "INSERT INTO game_players (game_id, player_id) SELECT $1, unnest($2::uuid[]) ON CONFLICT DO NOTHING"
        })
        {
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        foreach (var invite in invites)
        {
            command.Parameters.Clear();
            command.CommandText = """
                INSERT INTO invites (email, invite_id, game_id) VALUES ($1, $2, $3)
                ON CONFLICT (email, invite_id) DO NOTHING
                """;
            command.Parameters.AddWithValue(GameInviteRepository.NormalizeEmail(invite.Email));
            command.Parameters.AddWithValue(invite.InviteId);
            command.Parameters.AddWithValue(gameId);
            await command.ExecuteNonQueryAsync();
        }

        foreach (var player in state.PlayersStateModel.Players)
        {
            foreach (var other in state.PlayersStateModel.Players.Where(p => p.PlayerId != player.PlayerId))
            {
                command.Parameters.Clear();
                command.CommandText = """
                    INSERT INTO contacts (user_id, contact_id, name) VALUES ($1, $2, $3)
                    ON CONFLICT (user_id, contact_id) DO UPDATE
                        SET name = EXCLUDED.name, last_played = clock_timestamp()
                    """;
                command.Parameters.AddWithValue(player.PlayerId);
                command.Parameters.AddWithValue(other.PlayerId);
                command.Parameters.AddWithValue(other.PlayerName ?? string.Empty);
                await command.ExecuteNonQueryAsync();
            }
        }

        await transaction.CommitAsync();
        state.GameEtag = version.ToString(CultureInfo.InvariantCulture);
        state.LastUpdated = updatedAt;
    }

    public async Task<List<GameStateModel>> GetGameStates(GameStateFilterModel filterModel)
    {
        ArgumentNullException.ThrowIfNull(filterModel);
        await using var command = dataSource.CreateCommand("""
            SELECT g.state::text, g.version, g.updated_at FROM games g
            WHERE ($1 = '00000000-0000-0000-0000-000000000000'::uuid OR
                EXISTS (SELECT 1 FROM game_players p WHERE p.game_id = g.id AND p.player_id = $1 AND NOT p.hidden))
                AND ($2 OR g.privacy = $3)
            ORDER BY
                CASE WHEN $4 THEN g.updated_at END ASC,
                CASE WHEN NOT $4 THEN g.updated_at END DESC, g.id
            """);
        command.Parameters.AddWithValue(filterModel.PlayerId);
        command.Parameters.AddWithValue(filterModel.GamePrivacyFilter == GamePrivacyType.All);
        command.Parameters.AddWithValue((int)filterModel.GamePrivacyFilter);
        command.Parameters.AddWithValue(filterModel.Order == GameStateFilterModel.OrderBy.UpdatedAsc);
        await using var reader = await command.ExecuteReaderAsync();
        List<GameStateModel> results = [];
        while (await reader.ReadAsync())
        {
            results.Add(ReadState(reader));
        }

        return results;
    }

    public async Task DeleteGameStateIndex(Guid id, Guid playerId)
    {
        await using var command = dataSource.CreateCommand(
            "UPDATE game_players SET hidden = true WHERE game_id = $1 AND player_id = $2");
        command.Parameters.AddWithValue(id);
        command.Parameters.AddWithValue(playerId);
        await command.ExecuteNonQueryAsync();
    }

    private static GameStateModel ReadState(NpgsqlDataReader reader)
    {
        var state = JsonSerializer.Deserialize<GameStateModel>(reader.GetString(0), GameJson.Options)
            ?? throw new JsonException("Missing game snapshot.");
        state.GameEtag = reader.GetInt64(1).ToString(CultureInfo.InvariantCulture);
        state.LastUpdated = reader.GetFieldValue<DateTimeOffset>(2);
        return state;
    }
}
