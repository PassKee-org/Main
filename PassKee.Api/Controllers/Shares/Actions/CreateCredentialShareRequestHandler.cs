using System;
using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Shares;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Api.Controllers.Shares.Actions;

public class CreateCredentialShareRequestHandler : IAsyncRequestHandler<CreateCredentialShareRequest, CreateCredentialShareResponse>
{
    private readonly ICredentialShareDao _credentialShareDao;
    private readonly ICredentialDao _credentialDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;

    public CreateCredentialShareRequestHandler(
        ICredentialShareDao credentialShareDao,
        ICredentialDao credentialDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService)
    {
        _credentialShareDao = credentialShareDao;
        _credentialDao = credentialDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
    }

    public async Task<CreateCredentialShareResponse> ExecuteAsync(CreateCredentialShareRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();

        if (request.CredentialId.HasValue)
        {
            var cred = await _credentialDao.GetById(request.CredentialId.Value);
            if (cred != null)
            {
                await _securityService.CheckAccess(AccessLevel.Read, userId, cred);
            }
        }

        if (request.EncryptedBody == null || request.EncryptedBody.Length == 0)
        {
            throw new RecordNotFoundException("Encrypted body is required.");
        }

        var token = Guid.NewGuid().ToString("N");
        DateTime? expiresAt = request.ExpiresInSeconds.HasValue
            ? DateTime.UtcNow.AddSeconds(request.ExpiresInSeconds.Value)
            : null;

        var entity = new CredentialShareEntity
        {
            UserId = userId,
            CredentialId = request.CredentialId,
            Token = token,
            Type = request.Type,
            EncryptedBody = request.EncryptedBody,
            ExpiresAt = expiresAt,
            MaxViews = request.MaxViews,
            ViewCount = 0
        };

        await _credentialShareDao.CreateAsync(entity);

        return new CreateCredentialShareResponse
        {
            Token = entity.Token,
            ExpiresAt = entity.ExpiresAt,
            MaxViews = entity.MaxViews
        };
    }
}

