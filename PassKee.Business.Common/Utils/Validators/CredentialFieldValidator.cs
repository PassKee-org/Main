using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace PassKee.Business.Common.Utils.Validators;

/// <summary>
/// Provides validation and formatting utilities for credential fields (URLs, emails, phone numbers, and addresses).
/// </summary>
public static partial class CredentialFieldValidator
{
    private const int MinPhoneDigits = 5;
    private const int MinAddressLength = 3;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^[+]?[0-9\s\-().]+$")]
    private static partial Regex PhoneRegex();

    /// <summary>
    /// Checks whether the specified value represents a valid HTTP/HTTPS URL.
    /// Supports URLs without explicit scheme (e.g. "google.com"), localhost, and IP addresses.
    /// </summary>
    public static bool IsValidUrl([NotNullWhen(true)] string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var trimmed = url.Trim();
        if (trimmed.Contains(' ') || trimmed.Contains('\t') || trimmed.Contains('\r') || trimmed.Contains('\n'))
        {
            return false;
        }

        // Reject non-http schemes (e.g. ftp://, mailto:, javascript:)
        if (trimmed.Contains("://"))
        {
            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        else if (trimmed.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.StartsWith("tel:", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var candidate = trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : $"https://{trimmed}";

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        var host = uri.Host;
        if (string.IsNullOrWhiteSpace(host) || host.Contains(' '))
        {
            return false;
        }

        // Allow localhost, valid hostnames with at least one dot (domain.tld), and IP addresses
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return host.Contains('.') && !host.StartsWith('.') && !host.EndsWith('.') && !host.Contains("..");
    }

    /// <summary>
    /// Normalizes a URL by trimming whitespace and prepending "https://" if missing a scheme.
    /// Returns empty string if the input is null, empty, or whitespace.
    /// </summary>
    public static string NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        var trimmed = url.Trim();
        return trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : $"https://{trimmed}";
    }

    /// <summary>
    /// Validates the URL and outputs its normalized form if valid.
    /// </summary>
    public static bool TryNormalizeUrl(string? url, [NotNullWhen(true)] out string? normalizedUrl)
    {
        if (!IsValidUrl(url))
        {
            normalizedUrl = null;
            return false;
        }

        normalizedUrl = NormalizeUrl(url);
        return true;
    }

    /// <summary>
    /// Checks whether the specified value represents a valid email address.
    /// </summary>
    public static bool IsValidEmail([NotNullWhen(true)] string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var trimmed = email.Trim();
        return EmailRegex().IsMatch(trimmed);
    }

    /// <summary>
    /// Checks whether the specified value represents a valid phone number.
    /// Enforces phone character format and a minimum count of numeric digits.
    /// </summary>
    public static bool IsValidPhone([NotNullWhen(true)] string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return false;
        }

        var trimmed = phone.Trim();
        if (trimmed.Length < MinPhoneDigits)
        {
            return false;
        }

        if (!PhoneRegex().IsMatch(trimmed))
        {
            return false;
        }

        return trimmed.Count(char.IsAsciiDigit) >= MinPhoneDigits;
    }

    /// <summary>
    /// Checks whether the specified value represents a plausible physical or postal address.
    /// Requires minimum length and presence of letters or digits.
    /// </summary>
    public static bool IsValidAddress([NotNullWhen(true)] string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        var trimmed = address.Trim();
        if (trimmed.Length < MinAddressLength)
        {
            return false;
        }

        return trimmed.Any(char.IsLetterOrDigit);
    }

    /// <summary>
    /// Generates a Google Maps search URL for an address.
    /// </summary>
    public static string GetGoogleMapsUrl(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return string.Empty;
        }

        return $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(address.Trim())}";
    }
}
