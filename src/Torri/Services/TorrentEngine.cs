using System.Net;
using Microsoft.Extensions.Options;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Logging;
using MonoTorrent.PiecePicking;
using Torri.Configurations;
using Torri.Helpers;
using Torri.Models;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LoggerFactory = MonoTorrent.Logging.LoggerFactory;

namespace Torri.Services;

public interface ITorrentEngine
{
    Task<string> AddTorrentAsync(Torrent torrent, bool start);
    Task SetContentWantedAsync(string hash, List<int> wanted, List<int> unWanted);
    Task RemoveTorrentAsync(string hash);
    string GetTorrentName(string hash);
    Task StopAsync(string hash);
    Task StartAsync(string hash);
    Task PauseAsync(string hash);
    bool Exists(string hash);
    IEnumerable<T> ProjectTorrents<T>(Func<TorrentManager, T> projection);
    T ProjectTorrent<T>(string hash, Func<TorrentManager, T> projection);
    IEnumerable<ITorrentManagerFile> QueryTorrentContents(string hash);
    IEnumerable<(ITorrentManagerFile file, int index)> QueryVideoContents(string hash);
    MonoSettings GetUserSettings();
    Task UpdateSettingsAsync(MonoSettings userSettings);
    Task InitializeEngineAsync(List<string> startHashes);
    bool IsSingleFileTorrent(string hash);
}

public class TorrentEngine(
    IOptions<TorrentOptions> options,
    ILogger<TorrentEngine> logger) : ITorrentEngine
{
    private const string EngineStateFile = "data/engine.state";
    private ClientEngine _engine = null!;

    public async Task InitializeEngineAsync(List<string> startHashes)
    {
        logger.LogInformation("Torrent engine starting");
        LoggerFactory.Register(new MonoLogger(logger));

        _engine = File.Exists(EngineStateFile)
            ? await ClientEngine.RestoreStateAsync(EngineStateFile)
            : new ClientEngine();

        var userSettings = GetUserSettings();
        await UpdateSettingsAsync(userSettings);

        foreach (var manager in _engine.Torrents)
            await manager.ChangePickerAsync(new StandardPieceRequester(new PieceRequesterSettings(
                allowRandomised: false,
                allowPrioritisation: false,
                allowRarestFirst: false)));

        foreach (var startHash in startHashes)
        {
            var manager = _engine.Torrents.FirstOrDefault(x => x.InfoHashes.V1OrV2.ToHex() == startHash);
            if (manager != null)
            {
                logger.LogInformation("Starting torrent {hash}", startHash);
                await manager.StartAsync();
            }
        }
        
        logger.LogInformation("Torrent engine started");
    }

    public MonoSettings GetUserSettings()
    {
        return MonoSettingsMapper.ToUserSettings(_engine.Settings);
    }

    public async Task UpdateSettingsAsync(MonoSettings userSettings)
    {
        var builder = GetDefaultSettingsBuilder();
        MonoSettingsMapper.MapUserSettings(userSettings, builder);
        await _engine.UpdateSettingsAsync(builder.ToSettings());
        await _engine.SaveStateAsync(EngineStateFile);
    }

    public async Task<string> AddTorrentAsync(Torrent torrent, bool start)
    {
        if (!_engine.Contains(torrent))
        {
            var manager = await _engine.AddAsync(torrent, options.Value.DownloadsDirectory);

            if (start)
                await manager.StartAsync();
       
            await _engine.SaveStateAsync(EngineStateFile);
        }

        return torrent.InfoHashes.V1OrV2.ToHex();
    }

    public IEnumerable<T> ProjectTorrents<T>(Func<TorrentManager, T> projection)
    {
        return _engine.Torrents
            .Select(projection);
    }

    public T ProjectTorrent<T>(string hash, Func<TorrentManager, T> projection)
    {
        var manager = GetByHash(hash);
        return projection(manager);
    }

    public IEnumerable<ITorrentManagerFile> QueryTorrentContents(string hash)
    {
        var manager = GetByHash(hash);
        return manager.Files;
    }

    public bool IsSingleFileTorrent(string hash)
    {
        var manager = GetByHash(hash);
        return manager.Files.Count == 1;
    }

    public IEnumerable<(ITorrentManagerFile file, int index)> QueryVideoContents(string hash)
    {
        var manager = GetByHash(hash);

        var i = 0;
        foreach (var file in manager.Files)
        {
            if (FileHelper.IsVideoFile(file.Path))
                yield return (file, i);

            i++;
        }
    }

    public async Task RemoveTorrentAsync(string hash)
    {
        var manager = GetByHash(hash);
        await manager.StopAsync();
        await _engine.RemoveAsync(manager, RemoveMode.CacheDataAndDownloadedData);
        await _engine.SaveStateAsync(EngineStateFile);
    }

    public async Task SetContentWantedAsync(string hash, List<int> wanted, List<int> unWanted)
    {
        var manager = GetByHash(hash);

        foreach (var index in wanted)
            await manager.SetFilePriorityAsync(manager.Files[index], Priority.Normal);

        foreach (var index in unWanted)
            await manager.SetFilePriorityAsync(manager.Files[index], Priority.DoNotDownload);

        await manager.StartAsync();
        await _engine.SaveStateAsync(EngineStateFile);
    }
    
    public Task StopAsync(string hash)
    {
        var manager = GetByHash(hash);
        return manager.StopAsync();
    }

    public Task StartAsync(string hash)
    {
        var manager = GetByHash(hash);
        return manager.StartAsync();
    }

    public Task PauseAsync(string hash)
    {
        var manager = GetByHash(hash);
        return manager.PauseAsync();
    }
    
    public string GetTorrentName(string hash)
    {
        var manager = GetByHash(hash);
        return manager.Name;
    }

    public bool Exists(string hash)
    {
        return _engine.Torrents.Any(x => x.InfoHashes.V1OrV2.ToHex() == hash);
    }

    private TorrentManager GetByHash(string hash)
    {
        var manager = _engine.Torrents.First(x => x.InfoHashes.V1OrV2.ToHex() == hash);
        return manager;
    }

    private EngineSettingsBuilder GetDefaultSettingsBuilder()
    {
        return new EngineSettingsBuilder
        {
            AllowLocalPeerDiscovery = false,
            AllowPortForwarding = false,
            DhtEndPoint = null,
            UsePartialFiles = false,
            ListenEndPoints = new Dictionary<string, IPEndPoint>
            {
                { "ipv4", new IPEndPoint(IPAddress.Any, 51413) },
                { "ipv6", new IPEndPoint(IPAddress.IPv6Any, 51413) }
            },
            WebSeedDelay = TimeSpan.MaxValue
        };
    }
}

internal class MonoLogger(ILogger logger) : IRootLogger
{
    public void Info(string name, string message)
    {
        logger.LogInformation("{name}: {message}", name, message);
    }

    public void Debug(string name, string message)
    {
        logger.LogInformation("{name}: {message}", name, message);
    }

    public void Error(string name, string message)
    {
        logger.LogError("{name}: {message}", name, message);
    }
}