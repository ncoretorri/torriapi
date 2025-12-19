using Microsoft.AspNetCore.Http.HttpResults;
using Torri.Models;
using Torri.Services;

namespace Torri.Features.Settings;

public static partial class SettingsFeatures
{
    public static class Get
    {
        public static Ok<MonoSettings> Endpoint(ITorrentEngine torrentEngine)
        {
            var settings = torrentEngine.GetUserSettings();
            return TypedResults.Ok(settings);
        }
    }
}
