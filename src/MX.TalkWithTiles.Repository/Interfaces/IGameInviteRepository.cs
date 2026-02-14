using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MX.TalkWithTiles.Repository.Dtos;

namespace MX.TalkWithTiles.Repository.Interfaces;

public interface IGameInviteRepository
{
    Task UpdateGameInvite(Guid inviteId, string emailAddress, Guid gameId);
    Task<List<GameInviteDto>> GetGameInvites(string userEmail);
}