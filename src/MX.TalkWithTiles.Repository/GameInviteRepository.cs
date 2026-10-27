using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MX.TalkWithTiles.Repository.Dtos;
using MX.TalkWithTiles.Repository.Interfaces;
using Npgsql;

namespace MX.TalkWithTiles.Repository;

public sealed class GameInviteRepository(NpgsqlDataSource dataSource) : IGameInviteRepository
{
    public async Task UpdateGameInvite(Guid inviteId, string emailAddress, Guid gameId)
    {
        await using var command = dataSource.CreateCommand("""
            INSERT INTO invites (email, invite_id, game_id) VALUES ($1, $2, $3)
            ON CONFLICT (email, invite_id) DO UPDATE SET game_id = EXCLUDED.game_id
            """);
        command.Parameters.AddWithValue(NormalizeEmail(emailAddress));
        command.Parameters.AddWithValue(inviteId);
        command.Parameters.AddWithValue(gameId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<GameInviteDto>> GetGameInvites(string userEmail)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT invite_id, email, game_id FROM invites WHERE email = $1 ORDER BY invite_id");
        command.Parameters.AddWithValue(NormalizeEmail(userEmail));
        await using var reader = await command.ExecuteReaderAsync();
        List<GameInviteDto> result = [];
        while (await reader.ReadAsync())
        {
            result.Add(new GameInviteDto
            {
                InviteId = reader.GetGuid(0),
                Email = reader.GetString(1),
                GameId = reader.GetGuid(2)
            });
        }

        return result;
    }

    public async Task DeleteGameInvite(Guid inviteId, string emailAddress)
    {
        await using var command = dataSource.CreateCommand(
            "DELETE FROM invites WHERE email = $1 AND invite_id = $2");
        command.Parameters.AddWithValue(NormalizeEmail(emailAddress));
        command.Parameters.AddWithValue(inviteId);
        await command.ExecuteNonQueryAsync();
    }

    internal static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
