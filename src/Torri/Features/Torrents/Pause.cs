using Microsoft.AspNetCore.Http.HttpResults;
using Torri.Services;

namespace Torri.Features.Torrents;

public static partial class TorrentFeatures
{
    public static class Pause
    {
        public static async Task<NoContent> Endpoint(string hash, ITorrentEngine torrentEngine)
        {
            await torrentEngine.PauseAsync(hash);
            return TypedResults.NoContent();
        }
    }
}
