using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions;
using PassKee.Business.Common.Utils;
using PassKee.Business.Dto.Auth;
using PassKee.Orm.Entities;

namespace PassKee.Business.Services.Auth;

public interface IAuthService : IDomainService
{
    Task<AuthResultDto> RegisterAsync(
        string email,
        byte[] authHash,
        byte[] authSalt,
        byte[] userPublicKey,
        byte[] encryptedUserPrivateKey,
        byte[] encryptedUserVaultKey,
        KdfParameters? kdfParams = null,
        CancellationToken cancellationToken = default
    );

    Task<AuthResultDto> LoginAsync(
        string email,
        byte[] authHash,
        CancellationToken cancellationToken = default
    );

    Task<UserEntity> GetLoginParamsAsync(
        string email,
        CancellationToken cancellationToken = default
    );

    Task<AuthResultDto> GenerateNewJwtToken(string accessTokenString, string? previousJwtToken = null, CancellationToken cancellationToken = default);

    Task<AuthResultDto> GenerateNewJwtToken(UserAccessTokenEntity accessToken, CancellationToken cancellationToken = default);
}
