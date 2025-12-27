using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Torri.Configurations;
using Torri.Data.Enums;

namespace Torri.Services;

public interface IFileService
{
    string RemoveForbiddenCharacters(string path);
}

public partial class FileService : IFileService
{
    public string RemoveForbiddenCharacters(string path)
    {
        return InvalidCharsRegex().Replace(path, string.Empty);
    }
    
    [GeneratedRegex(@"[\<\>\:\""\/\\\|\?\*]")]
    private static partial Regex InvalidCharsRegex();
}