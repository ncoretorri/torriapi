using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Torri.Configurations;
using Torri.Data;
using Torri.Data.Entities;
using Torri.Data.Enums;
using Torri.Models;

namespace Torri.Services;

public interface IKodiCheckerService
{
    Task CheckAllAsync(CancellationToken cancellationToken);
}

public class KodiCheckerService(
    IOptions<TorrentOptions> options,
    ITorrentEngine torrentEngine,
    IKodiService kodiService,
    TorriContext context,
    ILogger<KodiCheckerService> logger)
    : IKodiCheckerService
{
    public async Task CheckAllAsync(CancellationToken cancellationToken)
    {
        var torrents = await context.Torrents
            .Where(x => x.AddToKodi && !x.IsProcessed)
            .Where(x => x.TorrentType != TorrentType.Serie || !x.HasMissingRegex)
            .Select(x => new TorrentEntity
            {
                Id = x.Id,
                TorrentName = x.TorrentName,
                DisplayName = x.DisplayName,
                Hash = x.Hash,
                TorrentType = x.TorrentType
            })
            .ToListAsync(cancellationToken);

        foreach (var torrent in torrents)
        {
            try
            {
                logger.LogInformation("Checking torrent {Name}", torrent.DisplayName);
                if (!torrentEngine.Exists(torrent.Hash))
                {
                    logger.LogInformation("Removing torrent from database because it does not exists in engine");
                    await context.Torrents
                        .Where(x => x.Id == torrent.Id)
                        .ExecuteDeleteAsync(cancellationToken);
                    continue;
                }

                await CheckAsync(torrent, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Checking torrent failed");
            }
        }
    }

    private async Task CheckAsync(TorrentEntity torrent, CancellationToken cancellationToken)
    {
        var files = await context.VideoFiles
            .Where(x => x.TorrentEntityId == torrent.Id)
            .Where(x => x.Wanted)
            .ToListAsync(cancellationToken);

        var filesData = torrentEngine.QueryVideoContents(torrent.Hash)
            .Select(x => new
            {
                x.file.Path,
                Progress = x.file.BitField.PercentComplete
            })
            .ToDictionary(x => x.Path);

        var serieMasks = torrent.TorrentType == TorrentType.Serie
            ? context.SerieMasks
                .Where(x => x.TorrentEntityId == torrent.Id)
                .OrderBy(x => x.Id)
                .Select(x => new SerieMask
                {
                    FixSeason = x.FixSeason,
                    Mask = x.Mask
                })
                .ToList()
            : [];
        
        serieMasks.ForEach(x => x.GenerateRegex(logger));

        var isAllInKodi = files.Count > 0;
        List<int> filesAddedToKodi = [];

        foreach (var file in files)
        {
            if (file.IsProcessed)
                continue;

            if (!filesData.TryGetValue(file.Path, out var fileData) || fileData.Progress < 10)
            {
                isAllInKodi = false;
                continue;
            }

            var downloadPath = torrentEngine.IsSingleFileTorrent(torrent.Hash)
                ? Path.Combine(options.Value.LinkRoot, file.Path)
                : Path.Combine(options.Value.LinkRoot, torrent.TorrentName, file.Path);
            
            if (kodiService.AddFileToKodi(torrent.TorrentType, torrent.DisplayName, file.Path, downloadPath, serieMasks))
            {
                logger.LogInformation("Added file {Path} to Kodi", file.Path);
                filesAddedToKodi.Add(file.Id);
            }
            else
            {
                isAllInKodi = false;
            }
        }

        if (filesAddedToKodi.Count > 0)
            await context.VideoFiles
                .Where(x => x.TorrentEntityId == torrent.Id)
                .Where(x => filesAddedToKodi.Contains(x.Id))
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsProcessed, true), cancellationToken);

        if (isAllInKodi)
            await context.Torrents
                .Where(x => x.Id == torrent.Id)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsProcessed, true), cancellationToken);
    }
}