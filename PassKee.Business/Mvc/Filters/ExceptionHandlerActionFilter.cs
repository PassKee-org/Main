using System.Net;
using System.Security.Authentication;
using Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using PassKee.Business.Common.Constants;
using PassKee.Business.Common.Dto;
using PassKee.Business.Common.Exceptions;
using PassKee.Business.Services.Http;

namespace PassKee.Business.Mvc.Filters;

public class ExceptionHandlerActionFilter : ActionFilterAttribute
{
    private readonly ILogger<ExceptionHandlerActionFilter> _logger;
    private readonly IApiRequestService _apiRequestService;

    public ExceptionHandlerActionFilter(
        ILogger<ExceptionHandlerActionFilter> logger,
        IApiRequestService apiRequestService
    )
    {
        _logger = logger;
        _apiRequestService = apiRequestService;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var exception = context.Exception;
        if (exception != null)
        {
            var response = new JsonCommonResponse
            {
                Status = HttpResponseStatus.Fail
            };
            var statusCode = (int) HttpStatusCode.BadRequest;
            if (exception is AuthenticationException)
            {
                response.ErrorCode = exception.GetType().Name;
                response.Message = "User not authorized(action executing)";
                statusCode = (int)HttpStatusCode.Unauthorized;
            }
            else if (exception is IDomainException)
            {
                response.ErrorCode = exception.GetType().Name;
                response.Message = exception.Message;
            }
            else
            {
                _logger.LogError(
                    exception,
                    exception.Message,
                    new Dictionary<string, object>
                    {
                        { "LoggedInUserId", _apiRequestService.IsAuthorized() ? _apiRequestService.GetCurrentUserId().ToString() : string.Empty },
                        { "RequestUrl", _apiRequestService.GetRequestUrl() ?? string.Empty },
                    }
                );
                statusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "Server error";
            }
            
            var badResponse = new BadRequestObjectResult(response)
            {
                StatusCode = statusCode
            };
            context.Result = badResponse;
            context.ExceptionHandled = true;
        }
        
        // Code to execute after the action
        base.OnActionExecuted(context);
    }
}
