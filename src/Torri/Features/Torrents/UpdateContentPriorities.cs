using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Torri.Data;
using Torri.Services;

namespace Torri.Features.Torrents;

public static partial class TorrentFeatures
{
    public static class UpdateContentPriorities
    {
        public class Request
        {
            public string Hash { get; set; } = string.Empty;
            public List<int> ActiveIndexes { get; set; } = [];
            public List<int> InactiveIndexes { get; set; } = [];
        }
        
        public static async Task<Results<NoContent, NotFound>> Endpoint(
            Request request,
            ITorrentEngine torrentEngine,
            TorriContext context,
            CancellationToken cancellationToken)
        {
            var torrentId = await context.Torrents
                .Where(x => x.Hash == request.Hash)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (torrentId == 0)
                return TypedResults.NotFound();

            await torrentEngine.SetContentWantedAsync(request.Hash, request.ActiveIndexes, request.InactiveIndexes);

            await context.VideoFiles
                .Where(x => x.TorrentEntityId == torrentId)
                .Where(x => request.ActiveIndexes.Contains(x.FileIndex))
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.Wanted, true), cancellationToken);

            await context.VideoFiles
                .Where(x => x.TorrentEntityId == torrentId)
                .Where(x => request.InactiveIndexes.Contains(x.FileIndex))
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.Wanted, false), cancellationToken);

            return TypedResults.NoContent();
        }
    }
}
