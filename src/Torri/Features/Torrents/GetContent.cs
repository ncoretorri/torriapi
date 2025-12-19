using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MonoTorrent;
using Torri.Data;
using Torri.Data.Entities;
using Torri.Services;
using TorrentType = Torri.Data.Enums.TorrentType;

namespace Torri.Features.Torrents;

public static partial class TorrentFeatures
{
    public static class GetContent
    {
        public class ContentData
        {
            public int Index { get; set; }
            public string Name { get; set; } = string.Empty;
            public long Size { get; set; }
            public bool Wanted { get; set; }
            public bool HasError { get; set; }
            public double PercentComplete { get; set; }
        }

        public static async Task<Results<NotFound, Ok<List<ContentData>>>> Endpoint(
            string hash,
            ITorrentEngine torrentEngine,
            TorriContext context,
            CancellationToken cancellationToken)
        {
            var torrent = await context.Torrents
                .Where(x => x.Hash == hash)
                .Select(x => new TorrentEntity
                {
                    Id = x.Id,
                    TorrentType = x.TorrentType
                })
                .FirstOrDefaultAsync(cancellationToken);
        
            if (torrent == null)
                return TypedResults.NotFound();
        
            var files = context.VideoFiles
                .Where(x => x.TorrentEntityId == torrent.Id)
                .Select(x => new
                {
                    x.Path,
                    HasError = x.Season == 0
                })
                .ToDictionary(x => x.Path, x => x.HasError);

            var contents = torrentEngine.QueryVideoContents(hash)
                .Select(x => new ContentData
                {
                    Size = x.file.Length,
                    Wanted = x.file.Priority != Priority.DoNotDownload,
                    Name = x.file.Path,
                    Index = x.index,
                    PercentComplete = x.file.BitField.PercentComplete,
                    HasError = torrent.TorrentType == TorrentType.Serie && (!files.TryGetValue(x.file.Path, out var hasError) || hasError)
                })
                .OrderBy(x => x.Name)
                .ToList();
        
            return TypedResults.Ok(contents);
        }
    }
}
