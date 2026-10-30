using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PassKee.Business.Common.Constants.Http;
using PassKee.Business.Services.Http;
using Xunit;

namespace PassKee.Tests.Unit.Services;

public class HttpCookiesServiceTests
{
    private static (HttpCookiesService Service, DefaultHttpContext Context) CreateService(
        string host = "localhost",
        bool isHttps = false,
        string[]? cookieDomains = null,
        string cookieKeyPostfix = "dev",
        string hostingEnvironment = "Development")
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", hostingEnvironment);

        var configValues = new Dictionary<string, string?>
        {
            ["App:Auth:CookieKeyPostfix"] = cookieKeyPostfix,
            ["App:Auth:AccessTokenLifetime"] = "30"
        };

        if (cookieDomains != null)
        {
            for (var i = 0; i < cookieDomains.Length; i++)
            {
                configValues[$"App:Auth:CookieDomains:{i}"] = cookieDomains[i];
            }
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var context = new DefaultHttpContext();
        context.Request.Host = new HostString(host);
        context.Request.Scheme = isHttps ? "https" : "http";

        var contextAccessor = new HttpContextAccessor
        {
            HttpContext = context
        };

        var service = new HttpCookiesService(configuration, contextAccessor);
        return (service, context);
    }

    [Fact]
    public void AppendAuthCookies_LocalhostHttp_SetsHostOnlyInsecureLaxCookieWithoutPostfix()
    {
        var (service, context) = CreateService(
            host: "localhost",
            isHttps: false,
            cookieDomains: [".passkee.org"],
            cookieKeyPostfix: "dev",
            hostingEnvironment: "Development");

        service.AppendAuthCookies("access-123", "jwt-456");

        var setCookies = context.Response.Headers.SetCookie.Select(c => c ?? string.Empty).ToArray();
        Assert.Equal(2, setCookies.Length);

        // Cookies should be named standard pk_access_token and pk_jwt_token without _dev on localhost
        Assert.Contains(setCookies, c => c.StartsWith("pk_access_token="));
        Assert.Contains(setCookies, c => c.StartsWith("pk_jwt_token="));

        // Domain should NOT be .passkee.org on localhost
        Assert.DoesNotContain(setCookies, c => c.Contains("domain=", StringComparison.OrdinalIgnoreCase));

        // Over HTTP, Secure should NOT be set
        Assert.DoesNotContain(setCookies, c => c.Contains("secure", StringComparison.OrdinalIgnoreCase));

        // Over HTTP on local, SameSite should be Lax
        Assert.Contains(setCookies, c => c.Contains("samesite=lax", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AppendAuthCookies_LocalhostHttps_SetsSecureSameSiteNoneCookie()
    {
        var (service, context) = CreateService(
            host: "localhost",
            isHttps: true,
            cookieDomains: [".passkee.org"],
            cookieKeyPostfix: "dev",
            hostingEnvironment: "Development");

        service.AppendAuthCookies("access-123", "jwt-456");

        var setCookies = context.Response.Headers.SetCookie.Select(c => c ?? string.Empty).ToArray();
        Assert.Equal(2, setCookies.Length);

        // Domain should not be present (host-only for localhost)
        Assert.DoesNotContain(setCookies, c => c.Contains("domain=", StringComparison.OrdinalIgnoreCase));

        // Under HTTPS, Secure should be present
        Assert.All(setCookies, c => Assert.Contains("secure", c, StringComparison.OrdinalIgnoreCase));

        // In Development under HTTPS, SameSite should be None for cross-port requests
        Assert.All(setCookies, c => Assert.Contains("samesite=none", c, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AppendAuthCookies_DevPasskeeHost_SetsDomainAndPostfix()
    {
        var (service, context) = CreateService(
            host: "dev-api.passkee.org",
            isHttps: true,
            cookieDomains: [".passkee.org"],
            cookieKeyPostfix: "dev",
            hostingEnvironment: "Development");

        service.AppendAuthCookies("access-123", "jwt-456");

        var setCookies = context.Response.Headers.SetCookie.Select(c => c ?? string.Empty).ToArray();
        Assert.Equal(2, setCookies.Length);

        // Postfix _dev should be applied on dev-api.passkee.org
        Assert.Contains(setCookies, c => c.StartsWith("pk_access_token_dev="));
        Assert.Contains(setCookies, c => c.StartsWith("pk_jwt_token_dev="));

        // Domain .passkee.org should be present
        Assert.All(setCookies, c => Assert.Contains("domain=.passkee.org", c, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AppendAuthCookies_Production_EnforcesLaxAndSecureAndDomain()
    {
        var (service, context) = CreateService(
            host: "api.passkee.org",
            isHttps: true,
            cookieDomains: [".passkee.org"],
            cookieKeyPostfix: "",
            hostingEnvironment: "Production");

        service.AppendAuthCookies("access-123", "jwt-456");

        var setCookies = context.Response.Headers.SetCookie.Select(c => c ?? string.Empty).ToArray();
        Assert.Equal(2, setCookies.Length);

        Assert.Contains(setCookies, c => c.StartsWith("pk_access_token="));
        Assert.Contains(setCookies, c => c.StartsWith("pk_jwt_token="));

        // Production strictly enforces domain, secure, and SameSite=Lax
        Assert.All(setCookies, c =>
        {
            Assert.Contains("domain=.passkee.org", c, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("secure", c, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("samesite=lax", c, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void Get_Fallback_FindsCookieWithOrWithoutPostfix()
    {
        var (service, context) = CreateService(
            host: "dev-api.passkee.org",
            isHttps: true,
            cookieDomains: [".passkee.org"],
            cookieKeyPostfix: "dev",
            hostingEnvironment: "Development");

        context.Request.Headers.Cookie = "pk_jwt_token=plain-jwt-value";

        var token = service.Get(HttpCookieKeyEnum.JwtToken);
        Assert.Equal("plain-jwt-value", token);
    }
}
