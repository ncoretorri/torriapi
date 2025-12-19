using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Torri.Configurations;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Torri.Services;

public interface IMovieDatabase
{
    bool IsActive { get; }
    Task<Info?> GetMovieTitleFromImdbId(string imdbId, CancellationToken cancellationToken);
}

public class Info
{
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Description { get; set; } = string.Empty;
}

public partial class TheMovieDatabaseService(HttpClient httpClient, IOptions<TmdbOptions> options) : IMovieDatabase
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public bool IsActive => !string.IsNullOrEmpty(options.Value.Key);

    public async Task<Info?> GetMovieTitleFromImdbId(string imdbId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(
            $"https://api.themoviedb.org/3/find/{imdbId}?external_source=imdb_id&language={options.Value.Language}",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<TmdbResponse>(Options, cancellationToken);
        if (data is null)
            return null;

        if (data.TvResults.Count == 1)
        {
            var result = data.TvResults[0];
            return new Info
            {
                Title = result.Name,
                Year = GetYearFromDate(result.Date),
                Description = result.Overview
            };
        }

        if (data.MovieResults.Count == 1)
        {
            var result = data.MovieResults[0];
            return new Info
            {
                Title = result.Title,
                Year = GetYearFromDate(result.Date),
                Description = result.Overview
            };
        }

        return null;
    }

    private static int GetYearFromDate(string date)
    {
        var match = YearRegex().Match(date);
        if (!match.Success)
            throw new ApplicationException("Year is invalid");

        return int.Parse(match.Value);
    }

    [GeneratedRegex(@"^\d{4}")]
    private static partial Regex YearRegex();

    public class TmdbResponse
    {
        [JsonPropertyName("movie_results")] public List<MovieResult> MovieResults { get; set; } = [];

        [JsonPropertyName("tv_results")] public List<TvResult> TvResults { get; set; } = [];
    }

    public class MovieResult
    {
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("release_date")] public string Date { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;
    }

    public class TvResult
    {
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("first_air_date")] public string Date { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;
    }
}