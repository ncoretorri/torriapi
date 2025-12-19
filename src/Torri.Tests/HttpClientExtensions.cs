using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Torri.Tests;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static Task<HttpResponseMessage> SendRequestAsync(
        this HttpClient client,
        HttpMethod method,
        string uri,
        object? data = null, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var request = new HttpRequestMessage(method, uri);
        return SendRequestAsync(client, request, data, expected);
    }

    public static Task<HttpResponseMessage> SendRequestAsync(
        this HttpClient client,
        HttpRequestMessage request,
        object? data = null,
        HttpStatusCode expected = HttpStatusCode.OK)
    {
        if (data != null)
        {
            var json = JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        }

        return SendRequestAsync(client, request, expected);
    }


    private static async Task<HttpResponseMessage> SendRequestAsync(
        this HttpClient client,
        HttpRequestMessage request,
        HttpStatusCode expected = HttpStatusCode.OK)
    {
        TestBase.Logger.LogInformation("Sending request");
        var response = await client.SendAsync(request);
        TestBase.Logger.LogInformation("StatusCode: {StatusCode} {StatusCodeStr}", (int)response.StatusCode,
            response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        TestBase.Logger.LogInformation("ResponseString: {ResponseString}", content);

        response.StatusCode.ShouldBe(expected);

        return response;
    }

    public static async Task<TResult?> SendRequestAsync<TResult>(
        this HttpClient client,
        HttpMethod method,
        string uri,
        object? data = null,
        HttpStatusCode expected = HttpStatusCode.OK)
        where TResult : class
    {
        var response = await SendRequestAsync(client, method, uri, data, expected);
        return await DeserializeResponse<TResult>(response, expected);
    }

    public static async Task<TResult?> SendRequestAsync<TResult>(
        this HttpClient client,
        HttpRequestMessage request,
        object? data = null,
        HttpStatusCode expected = HttpStatusCode.OK)
        where TResult : class
    {
        var response = await SendRequestAsync(client, request, data, expected);
        return await DeserializeResponse<TResult>(response, expected);
    }

    private static async Task<TResult?> DeserializeResponse<TResult>(
        HttpResponseMessage response,
        HttpStatusCode expected = HttpStatusCode.OK)
        where TResult : class
    {
        var content = await response.Content.ReadAsStringAsync();

        if (typeof(TResult) == typeof(string)) return content as TResult;

        if (response.StatusCode == expected && !string.IsNullOrEmpty(content))
        {
            var value = JsonSerializer.Deserialize<TResult>(content, JsonSerializerOptions);

            TestBase.Logger.LogInformation("ResponseObject: {@ResponseObject}",
                JsonSerializer.Serialize(value, JsonSerializerOptions));
            return value;
        }

        return null;
    }
}