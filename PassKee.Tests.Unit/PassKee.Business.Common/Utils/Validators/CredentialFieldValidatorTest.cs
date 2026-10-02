using PassKee.Business.Common.Utils.Validators;
using Xunit;

namespace PassKee.Tests.Unit.Business.Common.Utils.Validators;

public class CredentialFieldValidatorTest
{
    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com")]
    [InlineData("http://localhost")]
    [InlineData("http://localhost:3000")]
    [InlineData("http://localhost:8080/dashboard")]
    [InlineData("http://192.168.1.1")]
    [InlineData("http://192.168.1.1:8080/admin")]
    [InlineData("https://sub.domain.org/test/path?foo=bar#section")]
    [InlineData("example.com")]
    [InlineData("sub.example.co.uk/login")]
    [InlineData("my-vault.service.net:8443")]
    [InlineData("  https://trimmed.com/  ")]
    public void IsValidUrl_WhenValid_ReturnsTrue(string url)
    {
        Assert.True(CredentialFieldValidator.IsValidUrl(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-url")]
    [InlineData("justword")]
    [InlineData("http://")]
    [InlineData("https://")]
    [InlineData("ftp://example.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("mailto:test@example.com")]
    [InlineData("https://.example.com")]
    [InlineData("https://example.com.")]
    [InlineData("https://example..com")]
    [InlineData("https://example .com")]
    public void IsValidUrl_WhenInvalid_ReturnsFalse(string? url)
    {
        Assert.False(CredentialFieldValidator.IsValidUrl(url));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("example.com", "https://example.com")]
    [InlineData("example.com/path", "https://example.com/path")]
    [InlineData("http://example.com", "http://example.com")]
    [InlineData("https://example.com", "https://example.com")]
    [InlineData("HTTP://EXAMPLE.COM", "HTTP://EXAMPLE.COM")]
    [InlineData("  example.com  ", "https://example.com")]
    public void NormalizeUrl_ReturnsExpectedNormalizedString(string? input, string expected)
    {
        var result = CredentialFieldValidator.NormalizeUrl(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryNormalizeUrl_WhenValid_ReturnsTrueAndNormalizedUrl()
    {
        var success = CredentialFieldValidator.TryNormalizeUrl("github.com/org", out var normalized);
        Assert.True(success);
        Assert.Equal("https://github.com/org", normalized);
    }

    [Fact]
    public void TryNormalizeUrl_WhenInvalid_ReturnsFalseAndNull()
    {
        var success = CredentialFieldValidator.TryNormalizeUrl("invalid url with spaces", out var normalized);
        Assert.False(success);
        Assert.Null(normalized);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("john.doe@company.org")]
    [InlineData("first_last+tag@sub.domain.co.uk")]
    [InlineData("123@domain.io")]
    [InlineData("support@vault.app")]
    [InlineData("  admin@example.com  ")]
    public void IsValidEmail_WhenValid_ReturnsTrue(string email)
    {
        Assert.True(CredentialFieldValidator.IsValidEmail(email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("plainaddress")]
    [InlineData("@missinguser.com")]
    [InlineData("missingdomain@")]
    [InlineData("missing.tld@domain")]
    [InlineData("user name@example.com")]
    [InlineData("user@exa mple.com")]
    [InlineData("user@@example.com")]
    public void IsValidEmail_WhenInvalid_ReturnsFalse(string? email)
    {
        Assert.False(CredentialFieldValidator.IsValidEmail(email));
    }

    [Theory]
    [InlineData("+1 (555) 123-4567")]
    [InlineData("+44 20 7946 0958")]
    [InlineData("(123) 456-7890")]
    [InlineData("123-456-7890")]
    [InlineData("+79991234567")]
    [InlineData("0123456789")]
    [InlineData("8-800-555-35-35")]
    [InlineData("  +1 555 123 4567  ")]
    public void IsValidPhone_WhenValid_ReturnsTrue(string phone)
    {
        Assert.True(CredentialFieldValidator.IsValidPhone(phone));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("1234")]
    [InlineData("+--()")]
    [InlineData("not-a-phone")]
    [InlineData("+1 (555) call-me")]
    public void IsValidPhone_WhenInvalid_ReturnsFalse(string? phone)
    {
        Assert.False(CredentialFieldValidator.IsValidPhone(phone));
    }

    [Theory]
    [InlineData("123 Main St, Springfield")]
    [InlineData("Baker street 221B, London, UK")]
    [InlineData("Apt 4B")]
    [InlineData("Red Square, 1, Moscow")]
    [InlineData("  42 Wallaby Way, Sydney  ")]
    public void IsValidAddress_WhenValid_ReturnsTrue(string address)
    {
        Assert.True(CredentialFieldValidator.IsValidAddress(address));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    [InlineData("---")]
    [InlineData("... ,,,")]
    public void IsValidAddress_WhenInvalid_ReturnsFalse(string? address)
    {
        Assert.False(CredentialFieldValidator.IsValidAddress(address));
    }

    [Fact]
    public void GetGoogleMapsUrl_WhenAddressProvided_ReturnsCorrectQueryUrl()
    {
        var url = CredentialFieldValidator.GetGoogleMapsUrl("221B Baker St, London");
        Assert.Equal("https://www.google.com/maps/search/?api=1&query=221B%20Baker%20St%2C%20London", url);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetGoogleMapsUrl_WhenAddressNullOrWhitespace_ReturnsEmptyString(string? address)
    {
        var url = CredentialFieldValidator.GetGoogleMapsUrl(address);
        Assert.Equal(string.Empty, url);
    }
}
