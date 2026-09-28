using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Services.Vaults;
using PassKee.Business.Services.Http;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class UpdateDirectoryRequestHandler : IAsyncRequestHandler<UpdateDirectoryRequest, DirectoryResponse>
{
    private readonly IVaultService _vaultService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public UpdateDirectoryRequestHandler(IVaultService vaultService, IApiRequestService apiRequestService, IMapper mapper)
    {
        _vaultService = vaultService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<DirectoryResponse> ExecuteAsync(UpdateDirectoryRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var dir = await _vaultService.UpdateDirectoryAsync(userId, request.DirectoryId, request.ParentDirectoryId, request.EncryptedName);
        return new DirectoryResponse { Directory = _mapper.Map<DirectoryDto>(dir) };
    }
}
