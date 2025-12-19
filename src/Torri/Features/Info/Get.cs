using Microsoft.AspNetCore.Http.HttpResults;

namespace Torri.Features.Info;

public static class InfoFeatures
{
    public static class Get
    {
        public class InfoResponse
        {
            public long FreeSpace { get; init; }
        }
    
        public static Ok<InfoResponse> Endpoint(CancellationToken cancellationToken)
        {
            var info = DriveInfo.GetDrives().FirstOrDefault(x => x.Name == "/app/downloads");
    
            return TypedResults.Ok(new InfoResponse
            {
                FreeSpace = info?.AvailableFreeSpace ?? -1,
            });
        }
    }
}
