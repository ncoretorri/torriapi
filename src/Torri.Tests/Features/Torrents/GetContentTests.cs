using Torri.Data.Enums;
using Torri.Features.Torrents;

namespace Torri.Tests.Features.Torrents;

public class GetContentTests(TestServer server, ITestOutputHelper output) : TestBase(server, output)
{
    [Fact]
    public async Task ReturnsContent()
    {
        var hash = await CreateTorrent(TorrentType.Movie);
        var request = CreateRequest(TorrentFeatures.GetContent.Endpoint, x => x.UriParams.Add("hash", hash));

        var contents = await Client.SendRequestAsync<List<TorrentFeatures.GetContent.ContentData>>(request);
        var toCompare = new TorrentFeatures.GetContent.ContentData
        {
            HasError = false,
            Name = "video.mkv",
            Wanted = true,
            Index = 0,
            PercentComplete = 50,
            Size = 71
        };
        
        contents!.Count.ShouldBe(1);
        contents[0].ShouldBeEquivalentTo(toCompare);
    }
}