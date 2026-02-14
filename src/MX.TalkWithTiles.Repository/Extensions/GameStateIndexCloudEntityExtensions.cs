using System;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Repository.Models;

namespace MX.TalkWithTiles.Repository.Extensions;

internal static class GameStateIndexCloudEntityExtensions
{
    internal static string? BuildFilter(GameStateFilterModel filterModel)
    {
        string playerIdFilter = $"PartitionKey eq '{filterModel.PlayerId}'";
        string privacyTypeFilter = $"GamePrivacyTypeValue eq '{filterModel.GamePrivacyFilter}'";

        if (filterModel.PlayerId != Guid.Empty && filterModel.GamePrivacyFilter != GamePrivacyType.All)
            return $"{playerIdFilter} and {privacyTypeFilter}";
        else if (filterModel.PlayerId != Guid.Empty)
            return playerIdFilter;
        else if (filterModel.GamePrivacyFilter != GamePrivacyType.All)
            return privacyTypeFilter;

        return null;
    }
}