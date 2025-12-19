using Microsoft.EntityFrameworkCore;
using Torri.Data.Enums;

namespace Torri.Tests.Features.Torrents;

public class AddTests(TestServer server, ITestOutputHelper output) : TestBase(server, output)
{
    [Theory]
    [InlineData(TorrentType.Movie)]
    [InlineData(TorrentType.Serie)]
    public async Task AddTorrent(TorrentType torrentType)
    {
        var hash = await CreateTorrent(torrentType);

        var torrent = await TorriContext.Torrents
            .Where(x => x.Hash == hash)
            .Include(x => x.VideoFiles)
            .FirstOrDefaultAsync();
        
        torrent!.DisplayName.ShouldBe("Title (2025)");
        torrent.TorrentName.ShouldBe("temp");
        torrent.ExternalId.ShouldBe("ExternalId");
        torrent.TorrentType.ShouldBe(torrentType);
        torrent.VideoFiles.Count.ShouldBe(1);
        var videoFile = torrent.VideoFiles.First();
        videoFile.IsProcessed.ShouldBeFalse();
        videoFile.Wanted.ShouldBeTrue();
        videoFile.Path.ShouldBe("video.mkv");
    }
}