using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using MX.TalkWithTiles.Repository.Config;
using Npgsql;

namespace MX.TalkWithTiles.Web.Hosting;

public static class RuntimeConfiguration
{
    public static void LoadRuntimeConfiguration(this WebApplicationBuilder builder)
    {
        var path = Environment.GetEnvironmentVariable("TALKWITHTILES_RUNTIME_FILE")
            ?? "/run/secrets/runtime-configuration";
        builder.Configuration.AddJsonFile(path, optional: builder.Environment.IsDevelopment(), reloadOnChange: false);
        builder.Configuration.AddEnvironmentVariables();
    }

    public static void AddRuntimeServices(this WebApplicationBuilder builder, bool testingEnabled)
    {
        var database = builder.Configuration.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();
        builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(new NpgsqlConnectionStringBuilder
        {
            Host = database.Host,
            Port = database.Port,
            Database = database.Name,
            Username = database.Username,
            Password = File.ReadAllText(database.PasswordFile).TrimEnd('\r', '\n'),
            Timeout = 10,
            SslMode = SslMode.Disable,
            GssEncryptionMode = GssEncryptionMode.Disable,
            CommandTimeout = 30,
            IncludeErrorDetail = false
        }.ConnectionString));

        var keyPath = builder.Configuration["DataProtection:KeyPath"] ?? "/app/state/keys";
        builder.Services.AddDataProtection().SetApplicationName("TalkWithTiles")
            .PersistKeysToFileSystem(new DirectoryInfo(keyPath));

        var proxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
        if (!builder.Environment.IsDevelopment() && proxies.Length == 0)
        {
            throw new InvalidOperationException("Production requires explicit trusted proxy IP addresses.");
        }

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            if (proxies.Length > 0)
            {
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
                foreach (var proxy in proxies)
                {
                    options.KnownProxies.Add(IPAddress.Parse(proxy));
                }
            }
        });

        if (!testingEnabled)
        {
            var certificateFile = builder.Configuration["AzureAd:ClientCertificateFile"]
                ?? "/run/secrets/client-certificate";
            var certificate = X509CertificateLoader.LoadPkcs12(
                Convert.FromBase64String(File.ReadAllText(certificateFile)), null, X509KeyStorageFlags.EphemeralKeySet);
            if (!certificate.HasPrivateKey)
            {
                throw new InvalidOperationException("A confidential-client certificate with private key is required.");
            }

            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    builder.Configuration.Bind("AzureAd", options);
                    options.ClientSecret = null;
                    options.ClientCertificates = [CertificateDescription.FromCertificate(certificate)];
                })
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();
        }
    }
}
