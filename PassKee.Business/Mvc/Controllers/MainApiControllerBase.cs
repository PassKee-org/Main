using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Dto;

namespace PassKee.Business.Mvc.Controllers;

public class MainApiControllerBase: ApiControllerBase
{
    protected readonly ILogger<MainApiControllerBase> Logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MainApiControllerBase(ILifetimeScope scope) : base(
        scope.Resolve<IAsyncRequestBuilder>()
    )
    {
        Logger = scope.Resolve<ILogger<MainApiControllerBase>>();
        _httpContextAccessor = scope.Resolve<IHttpContextAccessor>();
    }

    public override Func<IActionResult> Success => () =>
    {
        var httpResponse = _httpContextAccessor.HttpContext?.Response;
        if (httpResponse != null)
        {
            // If Redirect was completed in action
            if (httpResponse.StatusCode is StatusCodes.Status302Found)
            {
                var redirectUrl = httpResponse.Headers.Location!.FirstOrDefault();
                if (string.IsNullOrEmpty(redirectUrl))
                    throw new Exception("Redirect URL was not configured but status code yes");
                return Redirect(redirectUrl);
            }
            if (httpResponse.StatusCode is StatusCodes.Status301MovedPermanently)
            {
                var redirectUrl = httpResponse.Headers.Location!.FirstOrDefault();
                if (string.IsNullOrEmpty(redirectUrl))
                    throw new Exception("Redirect URL was not configured but status code yes");
                return RedirectPermanent(redirectUrl);
            }
        }
        return new OkObjectResult(
            new JsonCommonResponse { Status = HttpResponseStatus.Ok }
        );
    };
}
