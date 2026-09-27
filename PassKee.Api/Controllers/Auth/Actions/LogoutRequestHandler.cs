using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Services.Http;
using PassKee.Orm.Dao;

namespace PassKee.Api.Controllers.Auth.Actions;

public class LogoutRequestHandler : IAsyncRequestHandler<LogoutRequest>
{
    private readonly IHttpCookiesService _cookiesService;
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserAccessTokenDao _accessTokenDao;

    public LogoutRequestHandler(
        IHttpCookiesService cookiesService,
        IApiRequestService apiRequestService,
        IUserAccessTokenDao accessTokenDao)
    {
        _cookiesService = cookiesService;
        _apiRequestService = apiRequestService;
        _accessTokenDao = accessTokenDao;
    }

    public async Task ExecuteAsync(LogoutRequest request)
    {
        var accessToken = _apiRequestService.GetAccessToken();
        if (!string.IsNullOrEmpty(accessToken))
        {
            var accessTokenEntity = await _accessTokenDao.GetByToken(accessToken);
            if (accessTokenEntity != null)
            {
                await _accessTokenDao.Delete(accessTokenEntity);
            }
        }
        else
        {
            var accessTokenId = _apiRequestService.GetAccessTokenIdFromJwt();
            if (accessTokenId.HasValue)
            {
                var accessTokenEntity = await _accessTokenDao.GetById(accessTokenId.Value);
                if (accessTokenEntity != null)
                {
                    await _accessTokenDao.Delete(accessTokenEntity);
                }
            }
        }

        _cookiesService.CleanUpAuthCookies();
    }
}
