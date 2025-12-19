using Microsoft.EntityFrameworkCore;
using Torri.Data.Enums;
using Torri.Features.Torrents;

namespace Torri.Tests.Features.Torrents;

public class DeleteTests(TestServer server, ITestOutputHelper output) : TestBase(server, output)
{
    [Fact]
    public async Task DeletesTorrent()
    {
        var hash = await CreateTorrent(TorrentType.Movie);
        var request = CreateRequest(TorrentFeatures.Delete.Endpoint, x => x.UriParams.Add("hash", hash));
        
        await Client.SendRequestAsync(request, null, HttpStatusCode.NoContent);

        var exists = await TorriContext.Torrents
            .Where(x => x.Hash == hash)
            .AnyAsync();
        
        exists.ShouldBeFalse();
    }
}