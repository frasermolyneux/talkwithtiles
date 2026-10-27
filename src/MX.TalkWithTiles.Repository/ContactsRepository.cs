using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MX.TalkWithTiles.Repository.Dtos;
using MX.TalkWithTiles.Repository.Interfaces;
using MX.TalkWithTiles.Repository.Models;
using Npgsql;

namespace MX.TalkWithTiles.Repository;

public sealed class ContactsRepository(NpgsqlDataSource dataSource) : IContactsRepository
{
    public async Task<List<ContactDto>> GetContacts(ContactsFilterModel filterModel)
    {
        ArgumentNullException.ThrowIfNull(filterModel);
        await using var command = dataSource.CreateCommand(
            "SELECT contact_id, name, last_played FROM contacts WHERE user_id = $1 ORDER BY name");
        command.Parameters.AddWithValue(filterModel.UserId);
        await using var reader = await command.ExecuteReaderAsync();
        List<ContactDto> result = [];
        while (await reader.ReadAsync())
        {
            result.Add(new ContactDto
            {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
                LastPlayed = reader.GetFieldValue<DateTimeOffset>(2)
            });
        }

        return result;
    }

    public async Task UpdateContact(Guid userId, Guid contactId, string contactName)
    {
        await using var command = dataSource.CreateCommand("""
            INSERT INTO contacts (user_id, contact_id, name) VALUES ($1, $2, $3)
            ON CONFLICT (user_id, contact_id) DO UPDATE SET name = EXCLUDED.name, last_played = clock_timestamp()
            """);
        command.Parameters.AddWithValue(userId);
        command.Parameters.AddWithValue(contactId);
        command.Parameters.AddWithValue(contactName);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteContact(Guid userId, Guid contactId)
    {
        await using var command = dataSource.CreateCommand(
            "DELETE FROM contacts WHERE user_id = $1 AND contact_id = $2");
        command.Parameters.AddWithValue(userId);
        command.Parameters.AddWithValue(contactId);
        await command.ExecuteNonQueryAsync();
    }
}
