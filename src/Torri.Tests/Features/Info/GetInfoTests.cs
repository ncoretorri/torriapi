using Torri.Features.Info;

namespace Torri.Tests.Features.Info;

public class GetInfoTests(TestServer server, ITestOutputHelper output) : TestBase(server, output)
{
    [Fact]
    public async Task ReturnsInfoResponse()
    {
        var request = CreateRequest(InfoFeatures.Get.Endpoint);
        var result = await Client.SendRequestAsync<InfoFeatures.Get.InfoResponse>(request);

        result.ShouldNotBeNull();
    }
}