namespace MX.TalkWithTiles.Repository.Config;

public class AppDataOptions
{
    public required string StorageAccountUri { get; set; }
    public required string ContactsTableName { get; set; }
    public required string GameInviteTableName { get; set; }
    public required string GameStateTableName { get; set; }
    public required string GameStateIndexTableName { get; set; }
    public required string TilesTableName { get; set; }
}