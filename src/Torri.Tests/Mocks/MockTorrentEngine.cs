using MonoTorrent;
using MonoTorrent.Client;
using Torri.Helpers;
using Torri.Models;
using Torri.Services;

namespace Torri.Tests.Mocks;

public class MockTorrentEngine : ITorrentEngine
{
    public Dictionary<string, Torrent> Torrents { get; } = [];
    
    public Task<string> AddTorrentAsync(Torrent torrent, bool start)
    {
        Torrents.Add(torrent.InfoHashes.V1OrV2.ToHex(), torrent);
        return Task.FromResult(torrent.InfoHashes.V1OrV2.ToHex());
    }

    public Task SetContentWantedAsync(string hash, List<int> wanted, List<int> unWanted)
    {
        throw new NotImplementedException();
    }

    public Task RemoveTorrentAsync(string hash)
    {
        Torrents.Remove(hash);
        return Task.CompletedTask;
    }

    public string GetTorrentName(string hash)
    {
        return Torrents[hash]
            .Name;
    }

    public Task AnnounceAsync(string hash, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(string hash)
    {
        return Task.CompletedTask;
    }

    public Task StartAsync(string hash)
    {
        return Task.CompletedTask;
    }

    public Task PauseAsync(string hash)
    {
        return Task.CompletedTask;
    }
    
    public bool Exists(string hash)
    {
        throw new NotImplementedException();
    }

    IEnumerable<T> ITorrentEngine.ProjectTorrents<T>(Func<TorrentManager, T> projection)
    {
        throw new NotImplementedException();
    }

    public List<T> ProjectTorrents<T>(Func<TorrentManager, T> projection)
    {
        throw new NotImplementedException();
    }

    public T ProjectTorrent<T>(string hash, Func<TorrentManager, T> projection)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ITorrentManagerFile> QueryTorrentContents(string hash)
    {
        return Torrents[hash].Files
            .Select(x => new MockTorrentManagerFile
            {
                Length = x.Length,
                Priority = Priority.Normal,
                Path = x.Path,
                BitField = new ReadOnlyBitField([true, false])
            });
    }

    IEnumerable<(ITorrentManagerFile file, int index)> ITorrentEngine.QueryVideoContents(string hash)
    {
        var i = 0;
        foreach (var file in Torrents[hash].Files)
        {
            if (FileHelper.IsVideoFile(file.Path))
                yield return (new MockTorrentManagerFile
                {
                    Length = file.Length,
                    Priority = Priority.Normal,
                    Path = file.Path,
                    BitField = new ReadOnlyBitField([true, false])
                }, i);

            i++;

        }
    }

    public MonoSettings GetUserSettings()
    {
        throw new NotImplementedException();
    }

    public Task UpdateSettingsAsync(MonoSettings userSettings)
    {
        throw new NotImplementedException();
    }

    public Task InitializeEngineAsync(List<string> startHashes)
    {
        return Task.CompletedTask;
    }

    public bool IsSingleFileTorrent(string hash)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ITorrentManagerFile> QueryVideoContents(string hash)
    {
        throw new NotImplementedException();
    }
}

public class MockTorrentManagerFile : ITorrentManagerFile
{
    public string Path { get; init; } = string.Empty;
    public int StartPieceIndex { get; set; }
    public int EndPieceIndex { get; set; }
    public int PieceCount { get; set; }
    public long Length { get; init; }
    public long Padding { get; set; }
    public long OffsetInTorrent { get; set; }
    public MerkleRoot PiecesRoot { get; set; }
    public ReadOnlyBitField BitField { get; init; } = null!;
    public string FullPath { get; set; } = string.Empty;
    public string DownloadCompleteFullPath { get; set; } = string.Empty;
    public string DownloadIncompleteFullPath { get; set; } = string.Empty;
    public Priority Priority { get; init; }
}