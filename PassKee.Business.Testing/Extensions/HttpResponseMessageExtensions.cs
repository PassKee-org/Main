using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Net.Http.Headers;

namespace PassKee.Business.Testing.Extensions;

public static class HttpResponseMessageExtensions
{
    public static string? GetSetCookieValue(this HttpResponseMessage response, string key)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
        {
            return null;
        }

        return setCookieValues
            .Select(cookieValue => SetCookieHeaderValue.Parse(cookieValue))
            .Where(item =>
                item.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)
                || item.Name.ToString().StartsWith($"{key}_", StringComparison.InvariantCultureIgnoreCase)
            )
            .Where(item => item.Value != null)
            .Select(item => WebUtility.UrlDecode(item.Value.Value))
            .FirstOrDefault();
    }

    public static (string Name, string Value)? GetSetCookie(this HttpResponseMessage response, string key)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
        {
            return null;
        }

        var header = setCookieValues
            .Select(cookieValue => SetCookieHeaderValue.Parse(cookieValue))
            .Where(item =>
                item.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)
                || item.Name.ToString().StartsWith($"{key}_", StringComparison.InvariantCultureIgnoreCase)
            )
            .FirstOrDefault(item => item.Value != null);

        if (header?.Value == null) return null;
        return (header.Name.ToString(), WebUtility.UrlDecode(header.Value.Value) ?? string.Empty);
    }
}
