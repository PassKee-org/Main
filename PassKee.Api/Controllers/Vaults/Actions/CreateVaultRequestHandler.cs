using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Vaults;
using PassKee.Business.Services.Http;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class CreateVaultRequestHandler : IAsyncRequestHandler<CreateVaultRequest, VaultResponse>
{
    private readonly IVaultService _vaultService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public CreateVaultRequestHandler(IVaultService vaultService, IApiRequestService apiRequestService, IMapper mapper)
    {
        _vaultService = vaultService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<VaultResponse> ExecuteAsync(CreateVaultRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var vault = await _vaultService.CreateVaultAsync(userId, request.Name, request.EncryptedVaultKey ?? System.Array.Empty<byte>());
        return new VaultResponse { Vault = _mapper.Map<VaultDto>(vault) };
    }
}
