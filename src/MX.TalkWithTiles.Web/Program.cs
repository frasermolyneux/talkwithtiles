using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web.UI;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.CoreEngine.Factories;
using MX.TalkWithTiles.Repository;
using MX.TalkWithTiles.Repository.Interfaces;
using MX.TalkWithTiles.Web;
using MX.Observability.ApplicationInsights.AspNetCore;
using MX.TalkWithTiles.Web.Hosting;

if (args.Contains("--healthcheck", StringComparer.Ordinal))
{
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
    try
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://127.0.0.1:8080/api/health/ready");
        request.Headers.Host = "talkwithtiles.com";
        using var response = await client.SendAsync(request);
        return response.IsSuccessStatusCode ? 0 : 1;
    }
    catch (HttpRequestException)
    {
        return 1;
    }
    catch (TaskCanceledException)
    {
        return 1;
    }
}

var builder = WebApplication.CreateBuilder(args);
builder.LoadRuntimeConfiguration();

// Application Insights (skip when no connection string is configured)
var aiConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(aiConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry();
    builder.Services.AddObservability();
}

// Authentication: use test cookie scheme in development when Testing__Enabled is set
var testingEnabled = builder.Environment.IsDevelopment()
    && string.Equals(builder.Configuration["Testing:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
builder.AddRuntimeServices(testingEnabled);
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options => options.HttpsPort = 443);
    builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme,
        options => options.Cookie.SecurePolicy = CookieSecurePolicy.Always);
}

if (testingEnabled)
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/sign-in";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
        });
}

// MVC
var mvcBuilder = builder.Services.AddControllersWithViews();
if (!testingEnabled)
{
    mvcBuilder.AddMicrosoftIdentityUI();
}
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}
builder.Services.AddRazorPages();

// Response compression
builder.Services.AddResponseCompression();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Repository options & services
builder.Services.AddSingleton<IAppDataRepository, AppDataRepository>();
builder.Services.AddSingleton<IGameStateRepository, GameStateRepository>();
builder.Services.AddSingleton<IGameInviteRepository, GameInviteRepository>();
builder.Services.AddSingleton<IContactsRepository, ContactsRepository>();

// Game engine factories
builder.Services.AddScoped<IGameEngineFactory, GameEngineFactory>();
builder.Services.AddScoped<ITileFactory, TileFactory>();
builder.Services.AddScoped<IPlayerFactory, PlayerFactory>();
builder.Services.AddScoped<IManagerFactory, ManagerFactory>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

var app = builder.Build();
await app.Services.GetRequiredService<IAppDataRepository>().Initialize();
app.UseForwardedHeaders();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/Index");
    app.UseHsts();
}

app.UseMiddleware<GameConcurrencyMiddleware>();
app.UseStatusCodePagesWithReExecute("/Error/PageNotFound");

app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/health"),
    branch => branch.UseHttpsRedirection());
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapHealthChecks("/api/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
}).AllowAnonymous();
app.MapHealthChecks("/api/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
}).AllowAnonymous();
app.MapInfoEndpoint();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await app.RunAsync();
return 0;
