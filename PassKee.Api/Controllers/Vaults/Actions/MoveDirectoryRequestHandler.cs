using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Services.Security;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Common.Exceptions.Common;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao.Vaults;

namespace PassKee.Api.Controllers.Vaults.Actions;

public class MoveDirectoryRequestHandler : IAsyncRequestHandler<MoveDirectoryRequest, DirectoryResponse>
{
    private readonly IDirectoryDao _directoryDao;
    private readonly ISecurityService _securityService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IMapper _mapper;

    public MoveDirectoryRequestHandler(
        IDirectoryDao directoryDao,
        ISecurityService securityService,
        IApiRequestService apiRequestService,
        IMapper mapper)
    {
        _directoryDao = directoryDao;
        _securityService = securityService;
        _apiRequestService = apiRequestService;
        _mapper = mapper;
    }

    public async Task<DirectoryResponse> ExecuteAsync(MoveDirectoryRequest request)
    {
        var userId = _apiRequestService.GetCurrentUserId();
        var dir = await _directoryDao.GetById(request.DirectoryId);
        await _securityService.CheckAccess(AccessLevel.Write, userId, dir);

        if (request.ParentDirectoryId.HasValue)
        {
            if (request.ParentDirectoryId.Value == request.DirectoryId)
            {
                throw new ValidationException("Cannot move a directory into itself.");
            }

            var parentDir = await _directoryDao.GetById(request.ParentDirectoryId.Value);
            await _securityService.CheckAccess(AccessLevel.Write, userId, parentDir);
            if (parentDir!.VaultId != dir!.VaultId)
            {
                throw new HasNoAccessException();
            }

            var visited = new HashSet<Guid> { dir!.Id };
            var checkId = request.ParentDirectoryId;
            while (checkId.HasValue)
            {
                if (checkId.Value == dir.Id)
                {
                    throw new ValidationException("Cannot move a directory into its descendant.");
                }

                var current = await _directoryDao.GetById(checkId.Value);
                if (current == null || !visited.Add(current.Id))
                {
                    break;
                }

                checkId = current.ParentDirectoryId;
            }
        }

        var updated = await _directoryDao.MoveAsync(dir!, request.ParentDirectoryId);
        return new DirectoryResponse { Directory = _mapper.Map<DirectoryDto>(updated) };
    }
}

