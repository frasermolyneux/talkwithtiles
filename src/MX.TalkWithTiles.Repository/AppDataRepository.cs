using System;
using System.Threading.Tasks;
using MX.TalkWithTiles.Repository.Interfaces;
using Npgsql;

namespace MX.TalkWithTiles.Repository;

public sealed class AppDataRepository(NpgsqlDataSource dataSource) : IAppDataRepository
{
    public async Task Initialize()
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        // Serialize schema initialization across overlapping old/new container starts.
        await using var command = new NpgsqlCommand("""
            SELECT pg_advisory_xact_lock(746982151);
            CREATE TABLE IF NOT EXISTS schema_version (version integer PRIMARY KEY);
            INSERT INTO schema_version VALUES (1) ON CONFLICT DO NOTHING;
            CREATE TABLE IF NOT EXISTS games (
                id uuid PRIMARY KEY,
                state jsonb NOT NULL,
                version bigint NOT NULL DEFAULT 1,
                updated_at timestamptz NOT NULL DEFAULT clock_timestamp(),
                privacy integer NOT NULL
            );
            CREATE TABLE IF NOT EXISTS game_players (
                game_id uuid NOT NULL REFERENCES games(id) ON DELETE CASCADE,
                player_id uuid NOT NULL,
                hidden boolean NOT NULL DEFAULT false,
                PRIMARY KEY (game_id, player_id)
            );
            CREATE INDEX IF NOT EXISTS game_players_by_player ON game_players(player_id, game_id);
            CREATE TABLE IF NOT EXISTS contacts (
                user_id uuid NOT NULL,
                contact_id uuid NOT NULL,
                name text NOT NULL,
                last_played timestamptz NOT NULL DEFAULT clock_timestamp(),
                PRIMARY KEY (user_id, contact_id)
            );
            CREATE TABLE IF NOT EXISTS invites (
                email text NOT NULL,
                invite_id uuid NOT NULL,
                game_id uuid NOT NULL REFERENCES games(id) ON DELETE CASCADE,
                PRIMARY KEY (email, invite_id)
            );
            CREATE INDEX IF NOT EXISTS invites_by_game ON invites(game_id);
            """, connection, transaction);
        await command.ExecuteNonQueryAsync();
        command.CommandText = "SELECT version FROM schema_version";
        var version = (int?)await command.ExecuteScalarAsync();
        if (version != 1)
        {
            throw new InvalidOperationException("Unsupported database schema version.");
        }

        await transaction.CommitAsync();
    }

    public async Task<Tuple<bool, string>> HealthCheck()
    {
        try
        {
            await using var command = dataSource.CreateCommand("""
                SELECT version FROM schema_version WHERE version = 1
                AND EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'games')
                AND EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'game_players')
                AND EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'invites')
                AND EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'contacts')
                AND NOT pg_is_in_recovery()
                AND current_setting('transaction_read_only') = 'off'
                """);
            return (int?)await command.ExecuteScalarAsync() == 1
                ? Tuple.Create(true, "Ready")
                : Tuple.Create(false, "Schema or database writer unavailable");
        }
        catch (NpgsqlException)
        {
            return Tuple.Create(false, "Database unavailable");
        }
    }
}
