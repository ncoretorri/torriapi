using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Serilog;
using Torri.Tests.Http;
using Swashbuckle.AspNetCore.SwaggerGen;
using Testcontainers.PostgreSql;
using Torri.Services;
using Torri.Tests.EfCore;
using Torri.Tests.Mocks;

namespace Torri.Tests;

public class TestServer : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("https_port", "443");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var root = Directory.GetCurrentDirectory();
            var fileProvider = new PhysicalFileProvider(root);
            config.AddJsonFile(fileProvider, "testsettings.json", false, false);
        });

        builder.ConfigureLogging((context, logging) =>
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(context.Configuration)
                .CreateLogger();

            logging.Services.RemoveAll<ILoggerFactory>();
            logging.Services.RemoveAll<ILoggerProvider>();
            logging.Services.AddSerilog();
        });

        builder.ConfigureTestServices(services =>
        {
            // Disable swagger looking for xml comments
            services.RemoveAll<IConfigureOptions<SwaggerGenOptions>>();
            services.AddSingleton<ILinkGeneratorService, LinkGeneratorService>();
            services.AddSingleton<ITorrentEngine, MockTorrentEngine>();
            services.AddSingleton<IDatabaseCleanupService, DatabaseCleanupService>();
        });

        StartDatabase().GetAwaiter().GetResult();
    }

    private async Task StartDatabase()
    {
        var db = new PostgreSqlBuilder()
            .WithDatabase("torridb")
            .WithUsername("test")
            .WithPassword("testpassword")
            .WithPortBinding(2345, 5432)
            .WithName("TorriTestDb")
            .WithAutoRemove(true)
            .Build();

        await db.StartAsync();
    }
}