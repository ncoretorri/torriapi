using Microsoft.AspNetCore.Http.HttpResults;
using Torri.Services;

namespace Torri.Features.Torrents;

public static partial class TorrentFeatures
{
    public static class GetProgress
    {
        public class Response
        {
            public double Progress { get; set; }
            public string Status { get; set; } = string.Empty;
            public long DownloadRate { get; set; }
            public long UploadRate { get; set; }
            public int Leechs { get; set; }
            public int Seeds { get; set; }
            public int Peers { get; set; }
        }

        public static Ok<Response> Endpoint(
            string hash,
            ITorrentEngine torrentEngine)
        {
            var progress = torrentEngine.ProjectTorrent(hash, manager => new Response
            {
                Progress = manager.Progress,
                Status = manager.State.ToString(),
                DownloadRate = manager.Monitor.DownloadRate,
                UploadRate = manager.Monitor.UploadRate,
                Peers = manager.Peers.Available,
                Leechs = manager.Peers.Leechs,
                Seeds = manager.Peers.Seeds
            });

            return TypedResults.Ok(progress);
        }
    }
}
