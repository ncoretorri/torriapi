using Microsoft.EntityFrameworkCore;
using Torri.Data.Enums;
using Torri.Features.Torrents;

namespace Torri.Tests.Features.Torrents;

public class StartTests(TestServer server, ITestOutputHelper output) : TestBase(server, output)
{
    [Fact]
    public async Task Start()
    {
        var hash = await CreateTorrent(TorrentType.Movie);
        
        var torrent = await TorriContext.Torrents
            .Where(x => x.Hash == hash)
            .FirstOrDefaultAsync();
        
        torrent!.IsRunning.ShouldBeFalse();
        
        var request = CreateRequest(TorrentFeatures.Start.Endpoint, x => x.UriParams.Add("hash", hash));
        await Client.SendRequestAsync(request, null, HttpStatusCode.NoContent);
        
        torrent = await TorriContext.Torrents
            .Where(x => x.Hash == hash)
            .FirstOrDefaultAsync();
        
        torrent!.IsRunning.ShouldBeTrue();
    }
}