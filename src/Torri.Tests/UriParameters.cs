namespace Torri.Tests;

public class UriParameters
{
    public Dictionary<string, string> UriParams { get; } = [];
    public Dictionary<string, string?> QueryParams { get; } = [];

    public UriParameters AddUriParam(string key, string value)
    {
        UriParams.Add(key, value);
        return this;
    }
    public UriParameters AddQueryParam(string key, string value)
    {
        QueryParams.Add(key, value);
        return this;
    }
}