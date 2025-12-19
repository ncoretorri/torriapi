using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
using Torri.Data;
using Torri.Data.Entities;
using Torri.Models;
using Torri.Services;

namespace Torri.Features.SerieMasks;

public static partial class SerieMasksFeatures
{
    public partial class Update
    {
        public class SerieMaskDto
        {
            public int? FixSeason { get; set; }
            public string Mask { get; set; } = string.Empty;
        }

        public class UpdateRequest
        {
            public string Hash { get; set; } = string.Empty;
            public List<SerieMaskDto> SerieMasks { get; set; } = [];
        }

        public class Response
        {
            public bool HasMissingRegex { get; set; }
        }

        [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
        public partial class Mapper
        {
            [MapperIgnoreTarget(nameof(SerieMaskEntity.Id))]
            [MapperIgnoreTarget(nameof(SerieMaskEntity.TorrentEntity))]
            public static partial SerieMaskEntity ToEntity(SerieMaskDto serieMasks, int torrentEntityId);
            
            public static partial SerieMask ToModel(SerieMaskDto serieMasks);
        }

        public static async Task<Results<NotFound, Ok<Response>>> Endpoint(
            UpdateRequest request,
            TorriContext context,
            ITorrentEngine torrentEngine,
            IKodiService kodiService,
            ILogger<Update> logger,
            CancellationToken cancellationToken)
        {
            var torrent = await context.Torrents
                .Where(x => x.Hash == request.Hash)
                .Select(x => new TorrentEntity
                {
                    Id = x.Id
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (torrent == null)
                return TypedResults.NotFound();

            var files = await context.VideoFiles
                .Where(x => x.TorrentEntityId == torrent.Id)
                .Select(x => new VideoFileEntity
                {
                    Id = x.Id,
                    Path = x.Path
                })
                .ToListAsync(cancellationToken);

            var hasErrors = ProcessMasks(request.SerieMasks, files, context, kodiService, logger);
        
            var serieMasksEntities = request.SerieMasks
                .Select(x => Mapper.ToEntity(x, torrent.Id))
                .ToList();

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await context.Torrents
                    .Where(x => x.Id == torrent.Id)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(p => p.HasMissingRegex, hasErrors), cancellationToken);

                await context.SerieMasks
                    .Where(x => x.TorrentEntityId == torrent.Id)
                    .ExecuteDeleteAsync(cancellationToken);
            
                await context.SerieMasks.AddRangeAsync(serieMasksEntities, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            return TypedResults.Ok(new Response
            {
                HasMissingRegex = hasErrors
            });
        }
        
        private static bool ProcessMasks(
            List<SerieMaskDto> serieMasks,
            List<VideoFileEntity> files,
            TorriContext context,
            IKodiService kodiService,
            ILogger logger)
        {
            var masks = serieMasks
                .Select(Mapper.ToModel)
                .ToList();
        
            masks.ForEach(x => x.GenerateRegex(logger));
            var hasError = false;

            foreach (var file in files)
            {
                var data = kodiService.ProcessFileName(masks, file.Path);
                context.BeginUpdate(file)
                    .UpdateProperty(x => x.EpisodeName, data.filename)
                    .UpdateProperty(x => x.Season, data.season);
            
                if (data.season == 0) hasError = true;
            }
        
            return hasError;
        }
    }
}