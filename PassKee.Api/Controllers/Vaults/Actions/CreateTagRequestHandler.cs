using System;
using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class CreateTagRequestHandler : IAsyncRequestHandler<CreateTagRequest, TagResponse>
{
    private readonly IVaultDao _vaultDao;
    private readonly ITagDao _tagDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public CreateTagRequestHandler(
        IVaultDao vaultDao,
        ITagDao tagDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _vaultDao = vaultDao;
        _tagDao = tagDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<TagResponse> ExecuteAsync(CreateTagRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var vault = await _vaultDao.GetById(request.VaultId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, vault);

        if (request.EncryptedName == null || request.EncryptedName.Length == 0)
        {
            throw new ArgumentException("EncryptedName cannot be empty.");
        }

        var tag = await _tagDao.CreateAsync(request.VaultId, request.EncryptedName);
        return new TagResponse { Tag = _mapper.Map<TagDto>(tag) };
    }
}
