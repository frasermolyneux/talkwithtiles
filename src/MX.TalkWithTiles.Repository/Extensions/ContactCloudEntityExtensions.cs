using MX.TalkWithTiles.Repository.Models;

namespace MX.TalkWithTiles.Repository.Extensions;

public static class ContactCloudEntityExtensions
{
    internal static string BuildFilter(ContactsFilterModel filterModel)
    {
        return $"PartitionKey eq '{filterModel.UserId}'";
    }
}
