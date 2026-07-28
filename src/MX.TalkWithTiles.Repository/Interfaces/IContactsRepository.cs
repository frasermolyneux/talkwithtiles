using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MX.TalkWithTiles.Repository.Dtos;
using MX.TalkWithTiles.Repository.Models;

namespace MX.TalkWithTiles.Repository.Interfaces;

public interface IContactsRepository
{
    Task<List<ContactDto>> GetContacts(ContactsFilterModel filterModel);
    Task UpdateContact(Guid userId, Guid contactId, string contactName);
    Task DeleteContact(Guid userId, Guid contactId);
}
