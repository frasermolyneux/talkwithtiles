using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MX.TalkWithTiles.Repository.Interfaces;

namespace MX.TalkWithTiles.Web.Controllers;

[AllowAnonymous]
public class HealthCheckController : Controller
{
    private readonly List<HealthCheckComponent> _healthCheckComponents = [];

    public HealthCheckController(
        IAppDataRepository appDataRepository
    )
    {
        _healthCheckComponents.Add(new HealthCheckComponent
        {
            Name = "appdata-repo",
            Critical = true,
            HealthFunc = async () =>
            {
                try
                {
                    return await appDataRepository.HealthCheck();
                }
                catch (Exception ex)
                {
                    return new Tuple<bool, string>(false, ex.Message);
                }
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> Status()
    {
        var result = new HealthCheckResponse();

        foreach (var healthCheckComponent in _healthCheckComponents)
        {
            var (isHealthy, additionalData) = await healthCheckComponent.HealthFunc.Invoke();

            result.Components.Add(new HealthCheckComponentStatus
            {
                Name = healthCheckComponent.Name,
                Critical = healthCheckComponent.Critical,
                IsHealthy = isHealthy,
                AdditionalData = additionalData
            });
        }

        var actionResult = new JsonResult(result);

        if (!result.IsHealthy)
        {
            actionResult.StatusCode = 503;
        }

        return actionResult;
    }

    public class HealthCheckResponse
    {
        public bool IsHealthy => Components.All(c => c.IsHealthy);

        public List<HealthCheckComponentStatus> Components { get; set; } = [];
    }


    public class HealthCheckComponent
    {
        public string Name { get; set; } = string.Empty;
        public bool Critical { get; set; }
        public required Func<Task<Tuple<bool, string>>> HealthFunc { get; set; }
    }

    public class HealthCheckComponentStatus
    {
        public string Name { get; set; } = string.Empty;
        public bool Critical { get; set; }
        public bool IsHealthy { get; set; }
        public string AdditionalData { get; set; } = string.Empty;
    }
}
