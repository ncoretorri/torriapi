using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Torri.Services;

namespace Torri.Features.Torrents;

public static partial class TorrentFeatures
{
    public partial class Info
    {
        public class Request
        {
            public string TorrentName { get; set; } = string.Empty;
            public string? ImdbLink { get; set; }
        }
        
        [GeneratedRegex(@"tt\d+")]
        private static partial Regex ImdbIdRegex();

        [GeneratedRegex(@"(20\d\d)|(19\d\d)")]
        private static partial Regex YearRegex();

        public static async Task<Ok<Services.Info>> Endpoint(
            Request request,
            IMovieDatabase movieDatabase,
            ILogger<Info> logger,
            CancellationToken cancellationToken)
        {
            var yearMatch = YearRegex().Match(request.TorrentName);
            var response = new Services.Info
            {
                Title = request.TorrentName,
                Year = yearMatch.Success ? int.Parse(yearMatch.Value) : DateTime.Now.Year
            };

            if (string.IsNullOrEmpty(request.ImdbLink) || !movieDatabase.IsActive)
                return TypedResults.Ok(response);

            var match = ImdbIdRegex().Match(request.ImdbLink);
            if (!match.Success) return TypedResults.Ok(response);

            try
            {
                var info = await movieDatabase.GetMovieTitleFromImdbId(match.Value, cancellationToken);
                return TypedResults.Ok(info ?? response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting info");
            }
        
            return TypedResults.Ok(response);
        }
    }
}
