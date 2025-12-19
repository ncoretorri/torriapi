using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Torri.Data;
using Torri.Data.Entities;
using Torri.Services;

namespace Torri.Features.Torrents;

public static partial class TorrentFeatures
{
    public static class Start
    {
        public static async Task<Results<NoContent, NotFound>> Endpoint(
            string hash,
            TorriContext context,
            ITorrentEngine torrentEngine,
            CancellationToken cancellationToken)
        {
            var torrent = await context.Torrents
                .Where(x => x.Hash == hash)
                .Select(x => new TorrentEntity
                {
                    Id = x.Id,
                })
                .FirstOrDefaultAsync(cancellationToken);
        
            if (torrent == null)
                return TypedResults.NotFound();

            await torrentEngine.StartAsync(hash);
            await context
                .BeginUpdate(torrent)
                .UpdateProperty(x => x.IsRunning, true)
                .SaveChangesAsync(cancellationToken);
            return TypedResults.NoContent();
        }
    }
}
