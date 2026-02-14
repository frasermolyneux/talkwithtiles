using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MX.TalkWithTiles.Repository.CloudEntities;
using MX.TalkWithTiles.Repository.Config;
using MX.TalkWithTiles.Repository.Dtos;
using MX.TalkWithTiles.Repository.Interfaces;
using MX.TalkWithTiles.Repository.Models;

namespace MX.TalkWithTiles.Repository;

public class ContactsRepository(IOptions<AppDataOptions> options, ILogger<ContactsRepository> logger) : AppDataRepository(options), IContactsRepository
{
    public async Task<List<ContactDto>> GetContacts(ContactsFilterModel filterModel)
    {
        List<ContactDto> result = [];

        var userId = filterModel.UserId.ToString();
        await foreach (var r in ContactsTable.QueryAsync<ContactCloudEntity>(x => x.PartitionKey == userId))
        {
            result.Add(new ContactDto
            {
                Id = Guid.Parse(r.RowKey),
                Name = r.ContactName,
                LastPlayed = r.Timestamp ?? DateTimeOffset.MinValue
            });
        }

        return result;
    }

    public async Task UpdateContact(Guid userId, Guid contactId, string contactName)
    {
        var contactCloudEntity = new ContactCloudEntity(userId, contactId, contactName);

        await ContactsTable.UpsertEntityAsync(contactCloudEntity, TableUpdateMode.Merge);
    }

    public async Task DeleteContact(Guid userId, Guid contactId)
    {
        try
        {
            await ContactsTable.DeleteEntityAsync(userId.ToString(), contactId.ToString(), ETag.All);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            logger.LogWarning(ex, "Contact entity not found for deletion: UserId={UserId}, ContactId={ContactId}", userId, contactId);
        }
    }
}