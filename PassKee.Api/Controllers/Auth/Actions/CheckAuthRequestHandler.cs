using System.Threading.Tasks;
using Api.Requests.Abstractions;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

namespace PassKee.Api.Controllers.Auth.Actions;

public class CheckAuthRequestHandler : IAsyncRequestHandler<CheckAuthRequest>
{
    public Task ExecuteAsync(CheckAuthRequest request)
    {
        return Task.CompletedTask;
    }
}
