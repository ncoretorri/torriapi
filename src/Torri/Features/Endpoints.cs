using Torri.Features.Info;
using Torri.Features.SerieMasks;
using Torri.Features.Settings;
using Torri.Features.Torrents;

namespace Torri.Features;

public static class Endpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapInfoEndpoints()
        {
            var group = endpoints.MapGroup("/Info")
                .WithTags("Info");

            group.MapGet("/", InfoFeatures.Get.Endpoint);

            return endpoints;
        }

        public IEndpointRouteBuilder MapSerieMaskEndpoints()
        {
            var group = endpoints.MapGroup("/SerieMasks")
                .WithTags("SerieMasks");

            group.MapGet("/{hash}", SerieMasksFeatures.List.Endpoint);
            group.MapPut("/",  SerieMasksFeatures.Update.Endpoint);

            return endpoints;
        }

        public IEndpointRouteBuilder MapSettingEndpoints()
        {
            var group = endpoints.MapGroup("/Settings")
                .WithTags("Settings");

            group.MapGet("/", SettingsFeatures.Get.Endpoint);
            group.MapPut("/", SettingsFeatures.Update.Endpoint);

            return endpoints;
        }

        public IEndpointRouteBuilder MapTorrentEndpoints()
        {
            var group = endpoints.MapGroup("/Torrent")
                .WithTags("Torrent");

            group.MapPost("/", TorrentFeatures.Add.Endpoint).DisableAntiforgery();
            group.MapDelete("/{hash}", TorrentFeatures.Delete.Endpoint);
            group.MapGet("/{hash}", TorrentFeatures.GetContent.Endpoint);
            group.MapPost("/info", TorrentFeatures.Info.Endpoint);
            group.MapGet("/progress/{hash}", TorrentFeatures.GetProgress.Endpoint);
            group.MapGet("/", TorrentFeatures.List.Endpoint);
            group.MapPost("/pause/{hash}", TorrentFeatures.Pause.Endpoint);
            group.MapPost("/start/{hash}", TorrentFeatures.Start.Endpoint);
            group.MapPost("/stop/{hash}", TorrentFeatures.Stop.Endpoint);

            return endpoints;
        }
    }
}