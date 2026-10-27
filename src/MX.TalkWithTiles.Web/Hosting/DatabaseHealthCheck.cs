using Microsoft.Extensions.Diagnostics.HealthChecks;
using MX.TalkWithTiles.Repository.Interfaces;

namespace MX.TalkWithTiles.Web.Hosting;

public sealed class DatabaseHealthCheck(IAppDataRepository repository) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var (healthy, description) = await repository.HealthCheck();
        return healthy ? HealthCheckResult.Healthy(description) : HealthCheckResult.Unhealthy(description);
    }
}
