using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.CoreEngine.Factories;
using MX.TalkWithTiles.Repository;
using MX.TalkWithTiles.Repository.Config;
using MX.TalkWithTiles.Repository.Interfaces;
using MX.TalkWithTiles.Web;

var builder = WebApplication.CreateBuilder(args);

// Application Insights (skip when no connection string is configured)
var aiConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(aiConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// Entra External ID authentication
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd");

// MVC
var mvcBuilder = builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();
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
builder.Services.Configure<AppDataOptions>(builder.Configuration.GetSection("AppData"));
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
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/Index");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/PageNotFound");

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

if (app.Environment.IsDevelopment())
{
    var repository = app.Services.GetRequiredService<IAppDataRepository>();
    await repository.CreateTablesIfNotExist();
}

app.MapHealthChecks("/api/health").AllowAnonymous();
app.MapInfoEndpoint();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await app.RunAsync();