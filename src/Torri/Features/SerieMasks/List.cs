using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Torri.Data;
using Torri.Data.Entities;

namespace Torri.Features.SerieMasks;

public static partial class SerieMasksFeatures
{
    public static class List
    {
        public class Response
        {
            public int? FixSeason { get; set; }
            public string Mask { get; set; } = string.Empty;
        }

        public static async Task<Results<NotFound, Ok<List<Response>>>> Endpoint(
            string hash,
            TorriContext context,
            CancellationToken cancellationToken)
        {
            var torrent = await context.Torrents
                .Where(x => x.Hash == hash)
                .Select(x => new TorrentEntity
                {
                    Id = x.Id
                })
                .FirstOrDefaultAsync(cancellationToken);
        
            if (torrent == null)
                return TypedResults.NotFound();

            var masks = await context.SerieMasks
                .Where(x => x.TorrentEntityId == torrent.Id)
                .Select(x => new Response
                {
                    FixSeason = x.FixSeason,
                    Mask = x.Mask
                })
                .ToListAsync(cancellationToken);

            return TypedResults.Ok(masks);
        }
    }
}
