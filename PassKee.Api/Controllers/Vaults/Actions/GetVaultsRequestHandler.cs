using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class GetVaultsRequestHandler : IAsyncRequestHandler<GetVaultsRequest, VaultsResponse>
{
    private readonly IVaultDao _vaultDao;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public GetVaultsRequestHandler(
        IVaultDao vaultDao,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _vaultDao = vaultDao;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<VaultsResponse> ExecuteAsync(GetVaultsRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var vaults = await _vaultDao.GetByUserId(userId);
        return new VaultsResponse
        {
            Vaults = _mapper.Map<List<VaultDto>>(vaults)
        };
    }
}
