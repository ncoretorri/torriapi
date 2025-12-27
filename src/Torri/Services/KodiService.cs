using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Torri.Configurations;
using Torri.Data.Enums;
using Torri.Models;

namespace Torri.Services;

public interface IKodiService
{
    (int season, string filename) ProcessFileName(List<SerieMask> masks, string fileName);

    bool AddFileToKodi(TorrentType type, string kodiName, string filePath, string downloadPath,
        List<SerieMask> serieMasks);

    void DeleteVideFilesFromKodi(TorrentType torrentType, string kodiName);
}

public class KodiService(
    IFileService fileService,
    IOptions<TorrentOptions> options) : IKodiService
{
    public bool AddFileToKodi(TorrentType type, string kodiName, string filePath, string downloadPath,
        List<SerieMask> serieMasks)
    {
        var name = fileService.RemoveForbiddenCharacters(kodiName);
        string? linkPath;

        switch (type)
        {
            case TorrentType.Movie:
            {
                var directory = Path.Combine(options.Value.MoviesDirectory, name);
                Directory.CreateDirectory(directory);

                linkPath = Path.Combine(directory, name) + Path.GetExtension(filePath);
                break;
            }

            case TorrentType.Serie:
            {
                var directory = Path.Combine(options.Value.SeriesDirectory, name);
                Directory.CreateDirectory(directory);

                var data = ProcessFileName(serieMasks, filePath);
                var seasonDirectory = Path.Combine(directory, "Season " + data.season);
                Directory.CreateDirectory(seasonDirectory);

                linkPath = Path.Combine(seasonDirectory, data.filename);
                break;
            }
            
            default:
                return false;
        }

        if (!File.Exists(linkPath))
            File.CreateSymbolicLink(linkPath, downloadPath);

        return true;
    }
    
    public void DeleteVideFilesFromKodi(TorrentType torrentType, string kodiName)
    {
        var name = fileService.RemoveForbiddenCharacters(kodiName);

        var kodiDirectory = torrentType switch
        {
            TorrentType.Movie => Path.Combine(options.Value.MoviesDirectory, name),
            TorrentType.Serie => Path.Combine(options.Value.SeriesDirectory, name),
            _ => null
        };

        if (kodiDirectory != null && Directory.Exists(kodiDirectory))
            Directory.Delete(kodiDirectory, true);
    }

    public (int season, string filename) ProcessFileName(List<SerieMask> masks, string fileName)
    {
        var season = 0;
        List<int> episodes = [];

        foreach (var mask in masks)
        {
            if (mask.FixSeason.HasValue)
                season = mask.FixSeason.Value;

            var m = mask.Regex.Match(fileName.Replace('.', ' '));
            if (!m.Success) continue;

            if (m.Groups.TryGetValue("s", out var seasonMatch)) season = int.Parse(seasonMatch.Value);

            if (episodes.Count == 0)
                foreach (Group group in m.Groups)
                    if (group.Name.StartsWith('e'))
                        episodes.Add(int.Parse(group.Value));

            if (season != 0 && episodes.Count > 0)
                break;
        }

        if (season == 0 || episodes.Count == 0)
            return (0, string.Empty);

        var kodiName = "S" + season.ToString("00") + "E" + string.Join("E", episodes.Select(x => x.ToString("00"))) +
                       Path.GetExtension(fileName);

        return (season, kodiName);
    }
}