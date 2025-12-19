using Torri.Features.Torrents;

namespace Torri.Tests.Features.Torrents;

public class GetInfoTests(TestServer server, ITestOutputHelper output) : TestBase(server, output)
{
    [Fact]
    public async Task ReturnsInfoWithoutTmdb()
    {
        var request = CreateRequest(TorrentFeatures.Info.Endpoint);
        var response = await Client.SendRequestAsync<Torri.Services.Info>(request, new TorrentFeatures.Info.Request
        {
            TorrentName = "temp 1998",
        });
        
        response!.Year.ShouldBe(1998);
        response.Title.ShouldBe("temp 1998");
        response.Description.ShouldBeNullOrEmpty();
    }
    
    [Fact]
    public async Task ReturnsInfoWithTmdb()
    {
        var request = CreateRequest(TorrentFeatures.Info.Endpoint);
        var response = await Client.SendRequestAsync<Torri.Services.Info>(request, new TorrentFeatures.Info.Request
        {
            TorrentName = "temp 1998",
            ImdbLink = "https://www.imdb.com/title/tt0108778/?ref_=fn_all_ttl_1"
        });
        
        response!.Year.ShouldBe(1994);
        response.Title.ShouldBe("Jóbarátok");
        response.Description.ShouldNotBeNullOrEmpty();
    }
}