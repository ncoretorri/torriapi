using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Torri.Data;
using Torri.Data.Entities;
using Torri.Services;

namespace Torri.Features.Torrents;

public static partial class TorrentFeatures
{
    public static class Delete
    {
        public static async Task<NoContent> Endpoint(
            string hash,
            TorriContext context,
            IFileService fileService,
            ITorrentEngine torrentEngine,
            IKodiService kodiService,
            CancellationToken cancellationToken)
        {
            await torrentEngine.RemoveTorrentAsync(hash);

            var torrent = await context.Torrents
                .Where(x => x.Hash == hash)
                .Select(x => new TorrentEntity
                {
                    TorrentType = x.TorrentType,
                    DisplayName = x.DisplayName,
                    TorrentName = x.TorrentName,
                    AddToKodi = x.AddToKodi
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (torrent != null)
            {
                if (torrent.AddToKodi)
                    kodiService.DeleteVideFilesFromKodi(torrent.TorrentType, torrent.DisplayName);

                await context.Torrents
                    .Where(x => x.Hash == hash)
                    .ExecuteDeleteAsync(cancellationToken);
            }

            return TypedResults.NoContent();
        }
    }
}
