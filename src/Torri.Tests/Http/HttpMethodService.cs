using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Torri.Tests.Http;

/// <summary>
/// A service that can determine the HTTP method of a controller action.
/// </summary>
public static class HttpMethodService
{
    private static readonly Dictionary<Type, HttpMethod> HttpAttributeMethodMap = new()
    {
        { typeof(HttpGetAttribute), HttpMethod.Get },
        { typeof(HttpPostAttribute), HttpMethod.Post },
        { typeof(HttpPutAttribute), HttpMethod.Put },
        { typeof(HttpPatchAttribute), HttpMethod.Patch },
        { typeof(HttpDeleteAttribute), HttpMethod.Delete },
        { typeof(HttpHeadAttribute), HttpMethod.Head },
        { typeof(HttpOptionsAttribute), HttpMethod.Options }
    };

    /// <summary>
    /// Returns the <see cref="HttpMethod"/> for a controller action.
    /// </summary>
    /// <param name="action">Controller action.</param>
    public static HttpMethod GetHttpMethodForAction(MethodInfo action)
    {
        foreach (var attribute in action.CustomAttributes)
            if (HttpAttributeMethodMap.TryGetValue(attribute.AttributeType, out var httpMethod))
                return httpMethod;

        throw new ArgumentException($"Could not determine {nameof(HttpMethod)} of action {action.Name}.");
    }
}