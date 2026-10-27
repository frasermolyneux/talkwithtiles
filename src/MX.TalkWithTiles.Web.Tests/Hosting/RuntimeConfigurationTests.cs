using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using MX.TalkWithTiles.Web.Controllers;
using MX.TalkWithTiles.Web.Hosting;
using MX.TalkWithTiles.Repository;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MX.TalkWithTiles.Web.Tests.Hosting;

public class RuntimeConfigurationTests
{
    [Fact]
    public void Runtime_ProductionRequiresKnownProxies()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        Assert.Throws<InvalidOperationException>(() => builder.AddRuntimeServices(testingEnabled: true));
    }

    [Fact]
    public void Runtime_TrustsOnlyConfiguredProxyAndNotForwardedHost()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ForwardedHeaders:KnownProxies:0"] = "172.20.0.2"
        });
        builder.AddRuntimeServices(testingEnabled: true);
        using var services = builder.Services.BuildServiceProvider();
        var options = services.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
        Assert.Equal(["172.20.0.2"], options.KnownProxies.Select(p => p.ToString()));
        Assert.Empty(options.KnownIPNetworks);
        Assert.Equal(1, options.ForwardLimit);
        Assert.False(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost));
    }

    [Theory]
    [InlineData("172.20.0.2", "https")]
    [InlineData("172.20.0.99", "http")]
    public async Task Runtime_RejectsSpoofedProxyHeaders(string remoteIp, string expectedScheme)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
            options.KnownProxies.Add(System.Net.IPAddress.Parse("172.20.0.2"));
        });
        await using var app = builder.Build();
        app.Use((context, next) =>
        {
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(remoteIp);
            return next(context);
        });
        app.UseForwardedHeaders();
        app.Run(context => context.Response.WriteAsync(context.Request.Scheme));
        await app.StartAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Host = "talkwithtiles.com";
        client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
        Assert.Equal(expectedScheme, await client.GetStringAsync("/"));
    }

    [Theory]
    [InlineData("Production", true)]
    [InlineData("Development", false)]
    public async Task TestAuth_RemainsDisabledOutsideExplicitDevelopmentOptIn(string environment, bool optIn)
    {
        var hostEnvironment = new Mock<IWebHostEnvironment>();
        hostEnvironment.SetupGet(e => e.EnvironmentName).Returns(environment);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Testing:Enabled"] = optIn.ToString()
        }).Build();
        var controller = new TestAuthController(hostEnvironment.Object, configuration);
        var result = await controller.SignIn(new TestSignInRequest
        {
            UserId = Guid.NewGuid().ToString(),
            UserName = "Test",
            Email = "test@example.invalid"
        });
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Concurrency_ConflictIsNotConvertedToGenericServerError()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.UseExceptionHandler(error => error.Run(context =>
        {
            context.Response.StatusCode = 500;
            return Task.CompletedTask;
        }));
        app.UseMiddleware<GameConcurrencyMiddleware>();
        app.Run(_ => throw new GameConcurrencyException());
        await app.StartAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Host = "talkwithtiles.com";
        using var response = await client.GetAsync("/");
        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public void Runtime_CertificateUsesConfidentialCodeRedemptionWithPersonalAccountRefreshScope()
    {
        var directory = Path.GetFullPath(Path.Combine(".ci-state", $"auth-{Guid.NewGuid():N}"));
        Directory.CreateDirectory(directory);
        try
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=ci-only.invalid", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
            var file = Path.Combine(directory, "certificate-base64");
            File.WriteAllText(file, Convert.ToBase64String(certificate.Export(X509ContentType.Pfx)));
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ForwardedHeaders:KnownProxies:0"] = "127.0.0.1",
                ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
                ["AzureAd:TenantId"] = "common",
                ["AzureAd:ClientId"] = "72ea5049-afe4-452a-92cc-a2c204a049e4",
                ["AzureAd:ClientCertificateFile"] = file
            });
            builder.AddRuntimeServices(testingEnabled: false);
            using var services = builder.Services.BuildServiceProvider();
            var options = services.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>()
                .Get(OpenIdConnectDefaults.AuthenticationScheme);
            Assert.Equal("code", options.ResponseType);
            Assert.Contains("offline_access", options.Scope);
            Assert.NotNull(services.GetRequiredService<Microsoft.Identity.Web.ITokenAcquisition>());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
