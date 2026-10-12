using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Models.Shares;

namespace PassKee.Web.Services.Http;

public partial class ApiService
{
    public async Task<CreateCredentialShareResponse?> CreateShareAsync(CreateCredentialShareRequest request)
    {
        return await PostAsync<CreateCredentialShareResponse>(ApiUrl.Shares, request);
    }

    public async Task<CredentialShareResponse?> GetShareAsync(string token)
    {
        return await GetAsync<CredentialShareResponse>(ApiUrl.Share(token));
    }
}
