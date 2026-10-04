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

public class UpdateTagRequestHandler : IAsyncRequestHandler<UpdateTagRequest, TagResponse>
{
    private readonly ITagDao _tagDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public UpdateTagRequestHandler(
        ITagDao tagDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _tagDao = tagDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<TagResponse> ExecuteAsync(UpdateTagRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var tag = await _tagDao.GetById(request.TagId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, tag);

        if (request.EncryptedName == null || request.EncryptedName.Length == 0)
        {
            throw new ArgumentException("EncryptedName cannot be empty.");
        }

        var updated = await _tagDao.UpdateAsync(tag!, request.EncryptedName);
        return new TagResponse { Tag = _mapper.Map<TagDto>(updated) };
    }
}
