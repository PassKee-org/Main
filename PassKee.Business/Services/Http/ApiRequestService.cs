using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PassKee.Business.Common.Exceptions;
using PassKee.Business.Services.Auth;
using PassKee.Orm.Dao;
using PassKee.Orm.Entities;

namespace PassKee.Business.Services.Http;

public class ApiRequestService: BaseApiRequestService, IApiRequestService
{
    private readonly IUserDao _userDao;

    public ApiRequestService(
        IHttpContextAccessor httpContext,
        IJwtAuthService jwtAuthService,
        IUserDao userDao,
        IHttpTokenResolverService httpTokenResolverService
    ): base(httpContext, jwtAuthService, httpTokenResolverService)
    {
        _userDao = userDao;
    }

    public async Task<UserEntity> GetCurrentUser()
    {
        var guid = GetCurrentUserId();
        var user = await _userDao.GetById(guid);
        if (user == null || user.DeletedAt != null)
        {
            throw DomainException.UserNotFoundException;
        }
        return user;
    }

    public async Task<UserEntity?> GetCurrentUserOrNull()
    {
        var guid = GetUserGuidFromJwt();
        if (guid == null)
            return null;

        var user = await _userDao.GetById(guid.Value);
        if (user == null || user.DeletedAt != null)
        {
            return null;
        }
        return user;
    }
}
