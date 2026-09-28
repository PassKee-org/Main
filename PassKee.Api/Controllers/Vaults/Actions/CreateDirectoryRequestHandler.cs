using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Vaults;
using PassKee.Business.Services.Http;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class CreateDirectoryRequestHandler : IAsyncRequestHandler<CreateDirectoryRequest, DirectoryResponse>
{
    private readonly IVaultService _vaultService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public CreateDirectoryRequestHandler(IVaultService vaultService, IApiRequestService apiRequestService, IMapper mapper)
    {
        _vaultService = vaultService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<DirectoryResponse> ExecuteAsync(CreateDirectoryRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var dir = await _vaultService.CreateDirectoryAsync(userId, request.VaultId, request.ParentDirectoryId, request.EncryptedName);
        return new DirectoryResponse { Directory = _mapper.Map<DirectoryDto>(dir) };
    }
}
