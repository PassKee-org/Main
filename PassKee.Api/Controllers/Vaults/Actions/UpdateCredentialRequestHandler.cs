using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Vaults;
using PassKee.Business.Services.Http;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class UpdateCredentialRequestHandler : IAsyncRequestHandler<UpdateCredentialRequest, CredentialResponse>
{
    private readonly IVaultService _vaultService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public UpdateCredentialRequestHandler(IVaultService vaultService, IApiRequestService apiRequestService, IMapper mapper)
    {
        _vaultService = vaultService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<CredentialResponse> ExecuteAsync(UpdateCredentialRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var cred = await _vaultService.UpdateCredentialAsync(userId, request.CredentialId, request.DirectoryId, (PassKee.Orm.Entities.Vaults.CredentialType)request.Type, request.EncryptedBody);
        return new CredentialResponse { Credential = _mapper.Map<CredentialDto>(cred) };
    }
}
