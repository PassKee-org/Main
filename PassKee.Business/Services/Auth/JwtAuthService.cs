using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace PassKee.Business.Services.Auth;

public class JwtAuthService : IJwtAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<IJwtAuthService> _logger;

    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _key;
    private readonly int _lifeTime;

    public JwtAuthService(
        IConfiguration configuration,
        ILogger<IJwtAuthService> logger
    )
    {
        _configuration = configuration;
        _logger = logger;
        _key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration.GetValue<string>("App:Auth:SymmetricSecurityKey") ?? "sDciuOhv2y4tXeM5Nd3t0cFBZB2S6P09q8k5ZmLXSC"
            )
        );
        _issuer = _configuration.GetValue<string>("App:Auth:Issuer") ?? "PassKee API";
        _audience = _configuration.GetValue<string>("App:Auth:Audience") ?? "PassKee API";
        _lifeTime = _configuration.GetValue<int>("App:Auth:JwtLifetime", 60);
    }

    public string BuildJwt(
        Guid userId,
        Guid? accessTokenId = null,
        DateTime? expirationTime = null,
        DateTime? notBeforeTime = null    
    )
    {
        var now = DateTime.UtcNow;
        expirationTime ??= now.Add(TimeSpan.FromMinutes(_lifeTime));
        notBeforeTime ??= now;
        var claims = new List<Claim>
        {
            new(ClaimsIdentity.DefaultNameClaimType, "user"),
            new(ClaimsIdentity.DefaultRoleClaimType, "user"),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Authentication, accessTokenId?.ToString() ?? string.Empty)
        };
        var signingCredentials = new SigningCredentials(
            _key,
            SecurityAlgorithms.HmacSha256
        );
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = _audience,
            NotBefore = notBeforeTime,
            Subject = new ClaimsIdentity(claims),
            Expires = expirationTime,
            SigningCredentials = signingCredentials
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenObject = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(tokenObject);
    }

    public Guid GetUserId(string jwtString)
    {
        ArgumentNullException.ThrowIfNull(jwtString);
        try
        {
            var jwt = new JwtSecurityToken(jwtString);
            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid");
            ArgumentNullException.ThrowIfNull(userIdClaim);
            return Guid.Parse(userIdClaim.Value);
        }
        catch (Exception)
        {
            return Guid.Empty;
        }
    }

    public bool IsValidJwt(string token, bool isValidateLifeTime = true)
    {
        ArgumentNullException.ThrowIfNull(token);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = isValidateLifeTime,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = _key
        };
        try
        {
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(
                token,
                parameters,
                out _
            );
        }
        catch (Exception e)
        {
            _logger.LogDebug($"Jwt Auth Token is Incorrect: {e.Message}", e);
            return false;
        }

        return true;
    }
    
    public bool IsJwt(string token)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        try
        {
            var jwt = jwtHandler.ReadJwtToken(token);
            return jwt != null;
        }
        catch
        {
            return false;
        }
    }
    
    public Guid? GetAccessTokenId(string jwtString)
    {
        return GetClaimValue<Guid>(ClaimTypes.Authentication, jwtString);
    }
    
    public bool IsTokenExpired(string token, TimeSpan? delayBefore = null)
    {
        var expirationTime = GetTokenExpirationTime(token);
        if (delayBefore != null)
        {
            expirationTime = expirationTime.Add(-delayBefore.Value);
        }
        return expirationTime < DateTime.UtcNow;
    }
    
    public DateTime GetTokenExpirationTime(string token)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        if (!jwtHandler.CanReadToken(token))
            throw new ArgumentException("Invalid JWT token");

        var jwtToken = jwtHandler.ReadJwtToken(token);
        return jwtToken.ValidTo;
    }
    
    private T? GetClaimValue<T>(string claimType, string jwtString)
    {
        ArgumentNullException.ThrowIfNull(jwtString);
        try
        {   
            var jwt = new JwtSecurityToken(jwtString);
            var value = jwt.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
            if (value == null)
                return default;
            if (typeof(T) == typeof(Guid))
            {
                if (Guid.TryParse(value, out var guidValue))
                {
                    return (T)Convert.ChangeType(guidValue, typeof(T));
                }
            }
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(value, out var boolValue))
                {
                    return (T)Convert.ChangeType(boolValue, typeof(T));
                }
            }
            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return default;
        }
        catch (Exception)
        {
            return default;
        }
    }
}

