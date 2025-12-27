using Torri.Features.Torrents;

namespace Torri.Tests.Features.Torrents;

public class GetInfoTests(TestServer server, ITestOutputHelper output) : TestBase(server, output)
{
    [Fact]
    public async Task ReturnsInfo()
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
}