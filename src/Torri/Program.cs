using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Torri.Configurations;
using Torri.Data;
using Torri.Features;
using Torri.HostedServices;
using Torri.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<TorrentOptions>(builder.Configuration.GetSection("Torrent"));
builder.Services.Configure<TmdbOptions>(builder.Configuration.GetSection("TMDB"));
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));

builder.Services.AddDbContext<TorriContext>((sp, options) =>
{
    var config = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    options.UseNpgsql(config.ConnectionString);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IKodiService, KodiService>();
builder.Services.AddSingleton<ITorrentEngine, TorrentEngine>();
builder.Services.AddScoped<ITorrentService, TorrentService>();

builder.Services.AddHttpClient<IMovieDatabase, TheMovieDatabaseService>(x =>
{
    var key = builder.Configuration["TMDB:Key"];
    x.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
});

builder.Services.AddHostedService<KodiCheker>();

var app = builder.Build();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app
    .MapInfoEndpoints()
    .MapSerieMaskEndpoints()
    .MapTorrentEndpoints()
    .MapSettingEndpoints();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting Torri");

    logger.LogInformation("Checking and running database migrations");
    var db = scope.ServiceProvider.GetRequiredService<TorriContext>();
    db.Database.Migrate();

    var runningTorrents = await db.Torrents
        .Where(x => x.IsRunning)
        .Select(x => x.Hash)
        .ToListAsync();
    
    await app.Services.GetRequiredService<ITorrentEngine>().InitializeEngineAsync(runningTorrents);
}

await app.RunAsync();

public partial class Program {}