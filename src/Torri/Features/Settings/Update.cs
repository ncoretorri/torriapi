using Microsoft.AspNetCore.Http.HttpResults;
using Torri.Models;
using Torri.Services;

namespace Torri.Features.Settings;

public static partial class SettingsFeatures
{
    public static class Update
    {
        public static async Task<NoContent> Endpoint(
            MonoSettings userSettings,
            ITorrentEngine torrentEngine)
        {
            await torrentEngine.UpdateSettingsAsync(userSettings);
            return TypedResults.NoContent();
        }
    }
}
