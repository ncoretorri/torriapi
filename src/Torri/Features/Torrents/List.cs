using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MonoTorrent.Client;
using Torri.Data;
using Torri.Data.Enums;
using Torri.Services;

namespace Torri.Features.Torrents;

public static partial class TorrentFeatures
{
    public static class List
    {
        public class Response
        {
            public string Hash { get; set; } = string.Empty;
            public double Progress { get; set; }
            public string DisplayName { get; set; } = string.Empty;
            public string TorrentName { get; set; } = string.Empty;
            public bool Paused { get; set; }
            public long Size { get; set; }
            public string ExternalId { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string TorrentType { get; set; } = string.Empty;
            public bool IsProcessed { get; set; }
            public bool HasError { get; set; }
        }
        
        public static async Task<Ok<List<Response>>> Endpoint(
            TorriContext context,
            ITorrentEngine torrentEngine,
            CancellationToken cancellationToken)
        {
            var dbTorrents = await context.Torrents
                .OrderByDescending(x => x.Id)
                .Select(x => new Response
                {
                    ExternalId = x.ExternalId,
                    DisplayName = x.DisplayName,
                    TorrentName = x.TorrentName,
                    TorrentType = x.TorrentType.ToString(),
                    IsProcessed = x.IsProcessed,
                    Hash = x.Hash,
                    HasError = x.TorrentType == TorrentType.Serie && x.HasMissingRegex
                })
                .ToListAsync(cancellationToken);
        
            var torrents = torrentEngine.ProjectTorrents(x => new Response
                {
                    Progress = x.Progress,
                    Paused = x.State == TorrentState.Paused,
                    Size = x.Torrent!.Size,
                    Status = x.State.ToString(),
                    Hash = x.InfoHashes.V1OrV2.ToHex()
                })
                .ToDictionary(x => x.Hash);

            foreach (var torrent in dbTorrents)
                if (torrents.TryGetValue(torrent.Hash, out var data))
                {
                    torrent.Progress = data.Progress;
                    torrent.Paused = data.Paused;
                    torrent.Size = data.Size;
                    torrent.Status = data.Status;
                }

            return TypedResults.Ok(dbTorrents);
        }
    }
}
