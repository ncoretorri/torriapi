using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MonoTorrent;
using Riok.Mapperly.Abstractions;
using Torri.Data;
using Torri.Data.Entities;
using Torri.Models;
using Torri.Services;
using TorrentType = Torri.Data.Enums.TorrentType;

namespace Torri.Features.Torrents;

public static partial class TorrentFeatures
{
    public partial class Add
    {
        public static readonly List<SerieMask> DefaultMasks =
        [
            new()
            {
                Mask = "s@@e##-e&&"
            },
            new()
            {
                Mask = "s@@e##-&&"
            },
            new()
            {
                Mask = "s@@e##&&"
            },
            new()
            {
                Mask = "s@@e##"
            }
        ];
        
        public class Request
        {
            public IFormFile File { get; set; } = null!;
            public TorrentType Type { get; set; }
            public string ExternalId { get; set; } = string.Empty;
            public string Title { get; set; } = null!;
            public int Year { get; set; }
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public bool Start { get; set; }
            public bool AddToKodi { get; set; }
        }

        public class Response
        {
            public string Hash { get; set; } = string.Empty;
        }

        [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
        public static partial class Mapper
        {
            [MapperIgnoreTarget(nameof(SerieMaskEntity.Id))]
            [MapperIgnoreTarget(nameof(SerieMaskEntity.TorrentEntity))]
            public static partial SerieMaskEntity ToEntity(SerieMask serieMasks, int torrentEntityId);
        }

        public static async Task<Ok<Response>> Endpoint(
            [FromForm] Request request,
            TorriContext context,
            ITorrentEngine torrentEngine,
            IKodiService kodiService,
            ILogger<Add> logger,
            CancellationToken cancellationToken)
        {
            var torrent = await Torrent.LoadAsync(request.File.OpenReadStream());

            var hash = await torrentEngine.AddTorrentAsync(torrent, request.Start);

            var filesQuery = torrentEngine.QueryVideoContents(hash);

            if (request.Type == TorrentType.Movie)
                filesQuery = filesQuery
                    .OrderByDescending(x => x.file.Length)
                    .Take(1);

            var masks = request.Type == TorrentType.Serie ? DefaultMasks : [];
            masks.ForEach(x => x.GenerateRegex(logger));

            var hasError = false;
            var torrentEntity = new TorrentEntity
            {
                AddToKodi = request.AddToKodi,
                Hash = hash,
                TorrentName = torrent.Name,
                DisplayName = $"{request.Title} ({request.Year})",
                ExternalId = request.ExternalId,
                TorrentType = request.Type,
                IsRunning = request.Start,
                VideoFiles = filesQuery.Select(x =>
                    {
                        var data = kodiService.ProcessFileName(masks, x.file.Path);
                        if (data.season == 0) hasError = true;
                        
                        return new VideoFileEntity
                        {
                            Path = x.file.Path,
                            FileIndex = x.index,
                            Wanted = true,
                            EpisodeName = data.filename,
                            Season = data.season
                        };
                    })
                .ToList(),
                HasMissingRegex = request.Type == TorrentType.Serie && hasError,
                SerieMasks = masks.Select(Mapper.ToEntity).ToList()
            };

            await context.AddAsync(torrentEntity, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return TypedResults.Ok(new Response
            {
                Hash = torrentEntity.Hash
            });
        }
    }
}