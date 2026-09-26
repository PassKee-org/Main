using System.Threading;
using System.Threading.Tasks;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Common.Exceptions.Api.Auth;
using PassKee.Business.Common.Utils;
using PassKee.Business.Dto.Auth;
using PassKee.Orm.Dao;
using PassKee.Orm.Entities;

namespace PassKee.Business.Services.Auth;

public class AuthService(IUserDao userDao, IJwtAuthService jwtAuthService) : IAuthService
{
    public async Task<AuthResultDto> RegisterAsync(
        string email,
        byte[] authHash,
        byte[] authSalt,
        byte[] userPublicKey,
        byte[] encryptedUserPrivateKey,
        byte[] encryptedUserVaultKey,
        string? kdfParams = null,
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

        return CreateAuthResult(user);
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

        return CreateAuthResult(user);
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

    private AuthResultDto CreateAuthResult(UserEntity user) =>
        new(jwtAuthService.BuildJwt(user.Id), user);
}
