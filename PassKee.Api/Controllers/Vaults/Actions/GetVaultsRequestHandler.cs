using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Vaults;
using PassKee.Business.Services.Http;
using System.Collections.Generic;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class GetVaultsRequestHandler : IAsyncRequestHandler<GetVaultsRequest, VaultsResponse>
{
    private readonly IVaultService _vaultService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public GetVaultsRequestHandler(
        IVaultService vaultService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _vaultService = vaultService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<VaultsResponse> ExecuteAsync(GetVaultsRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var vaults = await _vaultService.GetUserVaultsAsync(userId);
        return new VaultsResponse
        {
            Vaults = _mapper.Map<List<VaultDto>>(vaults)
        };
    }
}
