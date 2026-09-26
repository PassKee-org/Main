using System;
using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

namespace PassKee.Web.Services.Http;

public partial class ApiService
{
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest model)
    {
        return await PostAsync<AuthResponse>(ApiUrl.AuthRegister, model);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest model)
    {
        return await PostAsync<AuthResponse>(ApiUrl.AuthLogin, model);
    }

    public async Task<LoginParamsResponse?> GetLoginParamsAsync(string email)
    {
        return await GetAsync<LoginParamsResponse>($"{ApiUrl.AuthLoginParams}?email={Uri.EscapeDataString(email)}");
    }
}
