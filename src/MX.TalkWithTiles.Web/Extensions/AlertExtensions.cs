using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MX.TalkWithTiles.Web.Models;

namespace MX.TalkWithTiles.Web.Extensions;

public static class AlertExtensions
{
    private const string AlertKey = "Alerts";

    public static void AddAlertSuccess(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);

        alerts.Add(new Alert(message, "alert-success"));

        controller.TempData[AlertKey] = JsonSerializer.Serialize(alerts);
    }

    public static void AddAlertInfo(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);

        alerts.Add(new Alert(message, "alert-info"));

        controller.TempData[AlertKey] = JsonSerializer.Serialize(alerts);
    }

    public static void AddAlertWarning(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);

        alerts.Add(new Alert(message, "alert-warning"));

        controller.TempData[AlertKey] = JsonSerializer.Serialize(alerts);
    }

    public static void AddAlertDanger(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);

        alerts.Add(new Alert(message, "alert-danger"));

        controller.TempData[AlertKey] = JsonSerializer.Serialize(alerts);
    }

    private static ICollection<Alert> GetAlerts(Controller controller)
    {
        var alertsTemp = controller.TempData[AlertKey] ?? JsonSerializer.Serialize(new HashSet<Alert>());

        var alerts = JsonSerializer.Deserialize<ICollection<Alert>>(alertsTemp.ToString() ?? "[]") ??
                     new HashSet<Alert>();

        return alerts;
    }
}
