using System;
using System.Threading;
using System.Threading.Tasks;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Common.Exceptions.Api.Auth;
using PassKee.Business.Common.Utils;
using PassKee.Business.Dto.Auth;
using PassKee.Orm.Dao;
using PassKee.Orm.Entities;
using Persistence.Transactions.Behaviors;

namespace PassKee.Business.Services.Auth;

public class AuthService(
    IUserDao userDao,
    IJwtAuthService jwtAuthService,
    IUserAccessTokenDao accessTokenDao,
    IDbSessionProvider sessionProvider
) : IAuthService
{
    public async Task<AuthResultDto> RegisterAsync(
        string email,
        byte[] authHash,
        byte[] authSalt,
        byte[] userPublicKey,
        byte[] encryptedUserPrivateKey,
        byte[] encryptedUserVaultKey,
        KdfParameters? kdfParams = null,
        CancellationToken cancellationToken = default
    )
    {
        if (await userDao.GetByEmail(email, cancellationToken) != null)
        {
            throw new RecordIsExistsException("User with this email already exists.");
        }

        var serverHash = CryptoUtils.ComputeServerHash(authHash, authSalt);
        var user = await userDao.Create(
            email,
            authSalt,
            serverHash,
            userPublicKey,
            encryptedUserPrivateKey,
            encryptedUserVaultKey,
            kdfParams,
            cancellationToken
        );

        return await CreateAuthResult(user, cancellationToken);
    }

    public async Task<AuthResultDto> LoginAsync(
        string email,
        byte[] authHash,
        CancellationToken cancellationToken = default
    )
    {
        var user = await userDao.GetByEmail(email, cancellationToken);
        if (user?.ServerHash == null || user.AuthSalt == null || !CryptoUtils.VerifyServerHash(authHash, user.AuthSalt, user.ServerHash))
        {
            throw new UserNotAuthorizedException();
        }

        return await CreateAuthResult(user, cancellationToken);
    }

    public async Task<UserEntity> GetLoginParamsAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        var user = await userDao.GetByEmail(email, cancellationToken);
        if (user?.AuthSalt == null)
        {
            throw new UserNotFoundException();
        }

        return user;
    }

    public async Task<AuthResultDto> GenerateNewJwtToken(
        string accessTokenString,
        string? previousJwtToken = null,
        CancellationToken cancellationToken = default
    )
    {
        var accessToken = await accessTokenDao.GetByToken(accessTokenString, cancellationToken);
        if (
            accessToken == null
            || (
                !string.IsNullOrWhiteSpace(previousJwtToken)
                && !await accessTokenDao.HasJwtToken(accessToken, previousJwtToken, cancellationToken)
            )
        )
        {
            throw new UserNotAuthorizedException();
        }

        if (accessToken.ExpirationTime < DateTime.UtcNow)
        {
            throw new ExpiredJwtTokenException();
        }

        return await GenerateNewJwtToken(accessToken, cancellationToken);
    }

    public async Task<AuthResultDto> GenerateNewJwtToken(
        UserAccessTokenEntity accessToken,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(accessToken);

        if (accessToken.IsExpired)
        {
            await accessTokenDao.Delete(accessToken, cancellationToken);
            throw new ExpiredJwtTokenException();
        }

        await accessTokenDao.DeleteExpiredJwtTokens(accessToken, cancellationToken);

        var jwtToken = jwtAuthService.BuildJwt(accessToken.User.Id, accessToken.Id);
        var expirationTime = jwtAuthService.GetTokenExpirationTime(jwtToken);
        var jwtTokenEntity = new UserJwtTokenEntity
        {
            Token = jwtToken,
            CreatedAt = DateTime.UtcNow,
            ExpirationTime = expirationTime,
            AccessToken = accessToken
        };
        accessToken.JwtTokens.Add(jwtTokenEntity);
        await sessionProvider.CurrentSession.SaveAsync(jwtTokenEntity, cancellationToken);

        return new AuthResultDto(
            jwtTokenEntity.Token,
            accessToken.Token,
            accessToken.User
        );
    }

    private async Task<AuthResultDto> CreateAuthResult(UserEntity user, CancellationToken cancellationToken = default)
    {
        var accessToken = await accessTokenDao.CreateNew(user, cancellationToken);
        return await GenerateNewJwtToken(accessToken, cancellationToken);
    }
}
