using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Torri.Configurations;
using Torri.Data.Enums;

namespace Torri.Services;

public interface IFileService
{
    string RemoveForbiddenCharacters(string path);
    void DeleteTorrentFiles(TorrentType torrentType, string displayName, string name);
}

public partial class FileService(IOptions<TorrentOptions> options) : IFileService
{
    public string RemoveForbiddenCharacters(string path)
    {
        return InvalidCharsRegex().Replace(path, string.Empty);
    }

    public void DeleteTorrentFiles(TorrentType torrentType, string displayName, string name)
    {
        displayName = RemoveForbiddenCharacters(displayName);

        var kodiDirectory = torrentType switch
        {
            TorrentType.Movie => Path.Combine(options.Value.MoviesDirectory, displayName),
            TorrentType.Serie => Path.Combine(options.Value.SeriesDirectory, displayName),
            _ => null
        };

        if (kodiDirectory != null && Directory.Exists(kodiDirectory))
            Directory.Delete(kodiDirectory, true);

        var downloadPath = Path.Combine(options.Value.DownloadsDirectory, name);
        if (Directory.Exists(downloadPath))
            Directory.Delete(downloadPath, true);
    }

    [GeneratedRegex(@"[\<\>\:\""\/\\\|\?\*]")]
    private static partial Regex InvalidCharsRegex();
}