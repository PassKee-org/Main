using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Vaults;
using PassKee.Business.Services.Http;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class CreateCredentialRequestHandler : IAsyncRequestHandler<CreateCredentialRequest, CredentialResponse>
{
    private readonly IVaultService _vaultService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public CreateCredentialRequestHandler(IVaultService vaultService, IApiRequestService apiRequestService, IMapper mapper)
    {
        _vaultService = vaultService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<CredentialResponse> ExecuteAsync(CreateCredentialRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        // Assume mapping of enum types happens properly
        var cred = await _vaultService.CreateCredentialAsync(userId, request.VaultId, request.DirectoryId, (PassKee.Orm.Entities.Vaults.CredentialType)request.Type, request.EncryptedBody);
        return new CredentialResponse { Credential = _mapper.Map<CredentialDto>(cred) };
    }
}
