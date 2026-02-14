using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MX.TalkWithTiles.Web.Extensions;
using MX.TalkWithTiles.Web.Models;

namespace MX.TalkWithTiles.Web.Controllers;

public class ErrorController(ILogger<ErrorController> logger) : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        var httpContextTraceIdentifier = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var isAdmin = User.Identity?.IsAuthenticated == true && User.IsAdmin();

        var viewModel = new ErrorViewModel { RequestId = httpContextTraceIdentifier };

        try
        {
            var exceptionHandlerPathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature != null)
            {
                var path = exceptionHandlerPathFeature.Path;
                var exception = exceptionHandlerPathFeature.Error;

                logger.LogError(exception, "Error at path: '{Path}'", path);

                if (isAdmin)
                {
                    viewModel.ShowExceptionDetail = true;
                    viewModel.ExceptionPath = path;
                    viewModel.ExceptionMessage = exception.ToString();
                    viewModel.ExceptionStackTrace = exception.StackTrace;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to retrieve exception handler feature");
        }

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult PageNotFound()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CauseError()
    {
        throw new InvalidOperationException("This is a test error");
    }
}