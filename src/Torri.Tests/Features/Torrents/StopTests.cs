using Microsoft.EntityFrameworkCore;
using Torri.Data.Enums;
using Torri.Features.Torrents;

namespace Torri.Tests.Features.Torrents;

public class StopTests(TestServer server, ITestOutputHelper output) : TestBase(server, output)
{
    [Fact]
    public async Task Stop()
    {
        var hash = await CreateTorrent(TorrentType.Movie, true);
     
        var torrent = await TorriContext.Torrents
            .Where(x => x.Hash == hash)
            .FirstOrDefaultAsync();
        
        torrent!.IsRunning.ShouldBeTrue();
        
        var request = CreateRequest(TorrentFeatures.Stop.Endpoint, x => x.UriParams.Add("hash", hash));
        await Client.SendRequestAsync(request, null, HttpStatusCode.NoContent);
        
        torrent = await TorriContext.Torrents
            .Where(x => x.Hash == hash)
            .FirstOrDefaultAsync();
        
        torrent!.IsRunning.ShouldBeFalse();
    }
}