using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class CreateVaultRequestHandler : IAsyncRequestHandler<CreateVaultRequest, VaultResponse>
{
    private readonly IVaultDao _vaultDao;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public CreateVaultRequestHandler(IVaultDao vaultDao, IApiRequestService apiRequestService, IMapper mapper)
    {
        _vaultDao = vaultDao;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<VaultResponse> ExecuteAsync(CreateVaultRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var vault = await _vaultDao.CreateAsync(userId, request.Name, request.EncryptedVaultKey ?? System.Array.Empty<byte>());
        return new VaultResponse { Vault = _mapper.Map<VaultDto>(vault) };
    }
}
