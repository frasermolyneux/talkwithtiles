using System;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace MX.TalkWithTiles.Repository.Interfaces;

public interface IAppDataRepository
{
    TableClient ContactsTable { get; }
    TableClient GameInviteTable { get; }
    TableClient GameStateTable { get; }
    TableClient GameStateIndexTable { get; }
    TableClient TilesTable { get; }
    Task CreateTablesIfNotExist();
    Task<Tuple<bool, string>> HealthCheck();
}
