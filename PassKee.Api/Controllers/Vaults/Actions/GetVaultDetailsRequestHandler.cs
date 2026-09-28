using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Vaults;
using PassKee.Business.Services.Http;
using System.Collections.Generic;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class GetVaultDetailsRequestHandler : IAsyncRequestHandler<GetVaultDetailsRequest, VaultDetailsResponse>
{
    private readonly IVaultService _vaultService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public GetVaultDetailsRequestHandler(
        IVaultService vaultService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _vaultService = vaultService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<VaultDetailsResponse> ExecuteAsync(GetVaultDetailsRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        
        var vault = await _vaultService.GetVaultAsync(userId, request.VaultId);
        var directories = await _vaultService.GetVaultDirectoriesAsync(request.VaultId);
        var credentials = await _vaultService.GetVaultCredentialsAsync(request.VaultId);

        return new VaultDetailsResponse
        {
            Vault = _mapper.Map<VaultDto>(vault),
            Directories = _mapper.Map<List<DirectoryDto>>(directories),
            Credentials = _mapper.Map<List<CredentialDto>>(credentials)
        };
    }
}
