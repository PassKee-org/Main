using System;
using Domain.Abstractions;

namespace PassKee.Business.Services.Auth;

public interface IJwtAuthService : IDomainService
{
    string BuildJwt(
        Guid userId,
        Guid? accessTokenId = null,
        DateTime? expirationTime = null,
        DateTime? notBeforeTime = null    
    );
    public Guid GetUserId(string jwtString);
    bool IsValidJwt(string jwtString, bool isValidateLifeTime = true);
    bool IsJwt(string token);
    Guid? GetAccessTokenId(string jwtString);
    bool IsTokenExpired(string token, TimeSpan? delayBefore = null);
    DateTime GetTokenExpirationTime(string token);
}

