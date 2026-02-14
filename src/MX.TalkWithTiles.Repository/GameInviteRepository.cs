using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using MX.TalkWithTiles.Repository.CloudEntities;
using MX.TalkWithTiles.Repository.Config;
using MX.TalkWithTiles.Repository.Dtos;
using MX.TalkWithTiles.Repository.Extensions;
using MX.TalkWithTiles.Repository.Interfaces;

namespace MX.TalkWithTiles.Repository;

public class GameInviteRepository(IOptions<AppDataOptions> options) : AppDataRepository(options), IGameInviteRepository
{
    public async Task UpdateGameInvite(Guid inviteId, string emailAddress, Guid gameId)
    {
        var gameStateCloudEntity = new GameInviteEntity(inviteId, emailAddress.ToLower(), gameId);

        await GameInviteTable.UpsertEntityAsync(gameStateCloudEntity, TableUpdateMode.Merge);
    }

    public async Task<List<GameInviteDto>> GetGameInvites(string userEmail)
    {
        List<GameInviteDto> playerInvites = [];
        var normalizedEmail = userEmail.ToLower();

        await foreach (var entity in GameInviteTable.QueryAsync<GameInviteEntity>(x => x.PartitionKey == normalizedEmail))
        {
            playerInvites.Add(GameInviteEntityExtensions.ToDto(entity));
        }

        return playerInvites;
    }
}