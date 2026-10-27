using System;
using System.Threading.Tasks;

namespace MX.TalkWithTiles.Repository.Interfaces;

public interface IAppDataRepository
{
    Task Initialize();
    Task<Tuple<bool, string>> HealthCheck();
}
