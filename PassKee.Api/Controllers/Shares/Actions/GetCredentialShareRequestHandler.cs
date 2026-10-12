using System;
using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Shared.Models.Shares;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Shares.Actions;

public class GetCredentialShareRequestHandler : IAsyncRequestHandler<GetCredentialShareRequest, CredentialShareResponse>
{
    private readonly ICredentialShareDao _credentialShareDao;

    public GetCredentialShareRequestHandler(ICredentialShareDao credentialShareDao)
    {
        _credentialShareDao = credentialShareDao;
    }

    public async Task<CredentialShareResponse> ExecuteAsync(GetCredentialShareRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new RecordNotFoundException("Shared item not found or has expired.");
        }

        var entity = await _credentialShareDao.GetByTokenAsync(request.Token);
        if (entity == null)
        {
            throw new RecordNotFoundException("Shared item not found or has expired.");
        }

        if (entity.ExpiresAt.HasValue && DateTime.UtcNow > entity.ExpiresAt.Value)
        {
            await _credentialShareDao.DeletePermanentlyAsync(entity);
            throw new RecordNotFoundException("Shared item has expired.");
        }

        entity.ViewCount++;
        if (entity.MaxViews.HasValue && entity.ViewCount >= entity.MaxViews.Value)
        {
            await _credentialShareDao.DeletePermanentlyAsync(entity);
        }
        else
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }

        return new CredentialShareResponse
        {
            Type = entity.Type,
            EncryptedBody = entity.EncryptedBody,
            ExpiresAt = entity.ExpiresAt,
            CreatedAt = entity.CreatedAt
        };
    }
}

