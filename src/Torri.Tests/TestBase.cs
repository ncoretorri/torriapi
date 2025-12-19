using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Torri.Data;
using Torri.Data.Enums;
using Torri.Features.Torrents;
using Torri.Services;
using Torri.Tests.EfCore;
using Torri.Tests.Mocks;
using Torri.Tests.TestLogger;

namespace Torri.Tests;

[Collection("Integration Tests")]
public partial class TestBase(TestServer server, ITestOutputHelper output) : IAsyncLifetime
{
    private AsyncServiceScope _scope;
    private ReadOnlyCollection<Endpoint> _endpoints = null!;

    public static ILogger Logger { get; private set; } = null!;
    public static HttpClient Client { get; private set; } = null!;

    protected IServiceProvider Services { get; private set; } = null!;

    protected bool IsLoggingEnabled
    {
        set => TestSink.IsEnabled = value;
    }

    protected TorriContext TorriContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        TestSink.Output = output;

        Client = server.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost/")
        });
        Client.Timeout = TimeSpan.FromMinutes(5);
        _scope = server.Services.CreateAsyncScope();
        Services = _scope.ServiceProvider;
        
        TorriContext = Services.GetRequiredService<TorriContext>();
        _endpoints = Services.GetRequiredService<IEnumerable<EndpointDataSource>>().SelectMany(x => x.Endpoints).ToList().AsReadOnly();
        await Services.GetRequiredService<IDatabaseCleanupService>().CleanupAsync(TorriContext);
        var mockTorrentEngine = (MockTorrentEngine)Services.GetRequiredService<ITorrentEngine>();
        mockTorrentEngine.Torrents.Clear();

        Logger = (ILogger)Services.GetRequiredService(typeof(ILogger<>).MakeGenericType(GetType()));
    }
    
    protected async Task<string> CreateTorrent(TorrentType type, bool start = false)
    {
        var request = CreateRequest(TorrentFeatures.Add.Endpoint);
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(File.OpenRead("test.torrent")), nameof(TorrentFeatures.Add.Request.File), "test.torrent");
        content.Add(new StringContent(type.ToString()), nameof(TorrentFeatures.Add.Request.Type));
        content.Add(new StringContent("ExternalId"), nameof(TorrentFeatures.Add.Request.ExternalId));
        content.Add(new StringContent("Title"), nameof(TorrentFeatures.Add.Request.Title));
        content.Add(new StringContent("2025"), nameof(TorrentFeatures.Add.Request.Year));
        content.Add(new StringContent(start.ToString()), nameof(TorrentFeatures.Add.Request.Start));
        request.Content = content;

        var response = await Client.SendRequestAsync<TorrentFeatures.Add.Response>(request);
        return response!.Hash;
    }
    
    protected HttpRequestMessage CreateRequest(Delegate endpoint, Action<UriParameters>? parameters = null)
    {
        var action = (RouteEndpoint)_endpoints.First(x => (MethodInfo)x.Metadata[0] == endpoint.Method);
        var methodMetadata = (HttpMethodMetadata)action.Metadata.First(x => x is HttpMethodMetadata);

        var method = HttpMethod.Parse(methodMetadata.HttpMethods[0]);
        var uriParams = new UriParameters();
        parameters?.Invoke(uriParams);
        
        var route = ParamRegex().Replace(action.RoutePattern.RawText!, m =>
        {
            var key = m.Groups[1].Value;
            return !uriParams.UriParams.TryGetValue(key, out var value)
                ? throw new InvalidOperationException($"Missing uri parameter: {key}")
                : value;
        });

        var query = HttpUtility.ParseQueryString(string.Empty);
        foreach (var (key, value) in uriParams.QueryParams)
        {
            query.Add(key, value);
        }

        var uri = QueryHelpers.AddQueryString(route, uriParams.QueryParams);
        var request = new HttpRequestMessage(method, uri);
        
        return request;
    }
    
    public async Task DisposeAsync()
    {
        await Serilog.Log.CloseAndFlushAsync();
        await _scope.DisposeAsync();
    }

    [GeneratedRegex(@"{(\w+)}")]
    private static partial Regex ParamRegex();
}