using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Business.Services.Auth;

namespace PassKee.Api.Controllers.Auth.Actions;

public class GetLoginParamsRequestHandler : IAsyncRequestHandler<GetLoginParamsRequest, LoginParamsResponse>
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public GetLoginParamsRequestHandler(IAuthService authService, IMapper mapper)
    {
        _authService = authService;
        _mapper = mapper;
    }

    public async Task<LoginParamsResponse> ExecuteAsync(GetLoginParamsRequest request)
    {
        var user = await _authService.GetLoginParamsAsync(request.Email);
        return _mapper.Map<LoginParamsResponse>(user);
    }
}
