using System;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Options;
using MX.TalkWithTiles.Repository.Config;
using MX.TalkWithTiles.Repository.Interfaces;

namespace MX.TalkWithTiles.Repository;

public class AppDataRepository : IAppDataRepository
{
    private readonly TableServiceClient _tableServiceClient;

    public AppDataRepository(IOptions<AppDataOptions> options)
    {
        if (!string.IsNullOrEmpty(options.Value.StorageConnectionString))
        {
            _tableServiceClient = new TableServiceClient(options.Value.StorageConnectionString);
        }
        else
        {
            _tableServiceClient = new TableServiceClient(
                new Uri(options.Value.StorageAccountUri),
                new DefaultAzureCredential());
        }

        ContactsTable = _tableServiceClient.GetTableClient(options.Value.ContactsTableName);
        GameInviteTable = _tableServiceClient.GetTableClient(options.Value.GameInviteTableName);
        GameStateTable = _tableServiceClient.GetTableClient(options.Value.GameStateTableName);
        GameStateIndexTable = _tableServiceClient.GetTableClient(options.Value.GameStateIndexTableName);
        TilesTable = _tableServiceClient.GetTableClient(options.Value.TilesTableName);
    }

    public TableClient ContactsTable { get; }
    public TableClient GameInviteTable { get; }
    public TableClient GameStateTable { get; }
    public TableClient GameStateIndexTable { get; }
    public TableClient TilesTable { get; }

    public async Task CreateTablesIfNotExist()
    {
        await ContactsTable.CreateIfNotExistsAsync();
        await GameInviteTable.CreateIfNotExistsAsync();
        await GameStateTable.CreateIfNotExistsAsync();
        await GameStateIndexTable.CreateIfNotExistsAsync();
        await TilesTable.CreateIfNotExistsAsync();
    }

    public async Task<Tuple<bool, string>> HealthCheck()
    {
        try
        {
            await _tableServiceClient.GetPropertiesAsync();
            return new Tuple<bool, string>(true, "OK");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, ex.Message);
        }
    }
}
