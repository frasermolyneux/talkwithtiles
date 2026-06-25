using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MX.TalkWithTiles.Web.Controllers;
using MX.TalkWithTiles.Web.Models;

namespace MX.TalkWithTiles.Web.Tests.Controllers;

public class ErrorControllerTests
{
    private readonly Mock<ILogger<ErrorController>> _loggerMock = new();

    private ErrorController CreateController(ClaimsPrincipal? user = null, IExceptionHandlerPathFeature? exceptionFeature = null)
    {
        var httpContext = new DefaultHttpContext();

        if (user != null)
        {
            httpContext.User = user;
        }

        if (exceptionFeature != null)
        {
            httpContext.Features.Set(exceptionFeature);
        }

        var controller = new ErrorController(_loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        return controller;
    }

    private static ClaimsPrincipal CreateAdminUser()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "Admin User"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static ClaimsPrincipal CreateStandardUser()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "Standard User")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static ClaimsPrincipal CreateAnonymousUser()
    {
        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    private static Mock<IExceptionHandlerPathFeature> CreateExceptionFeature(string path = "/test/path", string message = "Test error")
    {
        var mock = new Mock<IExceptionHandlerPathFeature>();
        mock.Setup(f => f.Path).Returns(path);
        mock.Setup(f => f.Error).Returns(new InvalidOperationException(message));
        return mock;
    }

    [Fact]
    public void Index_AdminUser_WithException_ShowsExceptionDetail()
    {
        var exceptionFeature = CreateExceptionFeature("/game/create", "Something broke");
        var controller = CreateController(CreateAdminUser(), exceptionFeature.Object);

        var result = controller.Index() as ViewResult;

        var model = Assert.IsType<ErrorViewModel>(result?.Model);
        Assert.True(model.ShowExceptionDetail);
        Assert.Equal("/game/create", model.ExceptionPath);
        Assert.Contains("Something broke", model.ExceptionMessage);
    }

    [Fact]
    public void Index_StandardUser_WithException_HidesExceptionDetail()
    {
        var exceptionFeature = CreateExceptionFeature("/game/create", "Something broke");
        var controller = CreateController(CreateStandardUser(), exceptionFeature.Object);

        var result = controller.Index() as ViewResult;

        var model = Assert.IsType<ErrorViewModel>(result?.Model);
        Assert.False(model.ShowExceptionDetail);
        Assert.Null(model.ExceptionPath);
        Assert.Null(model.ExceptionMessage);
        Assert.Null(model.ExceptionStackTrace);
    }

    [Fact]
    public void Index_AnonymousUser_WithException_HidesExceptionDetail()
    {
        var exceptionFeature = CreateExceptionFeature();
        var controller = CreateController(CreateAnonymousUser(), exceptionFeature.Object);

        var result = controller.Index() as ViewResult;

        var model = Assert.IsType<ErrorViewModel>(result?.Model);
        Assert.False(model.ShowExceptionDetail);
        Assert.Null(model.ExceptionPath);
        Assert.Null(model.ExceptionMessage);
    }

    [Fact]
    public void Index_NoException_ReturnsModelWithRequestId()
    {
        var controller = CreateController(CreateStandardUser());

        var result = controller.Index() as ViewResult;

        var model = Assert.IsType<ErrorViewModel>(result?.Model);
        Assert.False(model.ShowExceptionDetail);
        Assert.True(model.ShowRequestId);
    }

    [Fact]
    public void Index_AdminUser_NoException_DoesNotShowExceptionDetail()
    {
        var controller = CreateController(CreateAdminUser());

        var result = controller.Index() as ViewResult;

        var model = Assert.IsType<ErrorViewModel>(result?.Model);
        Assert.False(model.ShowExceptionDetail);
    }

    [Fact]
    public void Index_WithException_LogsError()
    {
        var exceptionFeature = CreateExceptionFeature("/some/path", "Logged error");
        var controller = CreateController(CreateStandardUser(), exceptionFeature.Object);

        controller.Index();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
