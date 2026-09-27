using Fluxor;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

namespace PassKee.Web.Store.Auth;

[FeatureState]
public record AuthState
{
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    public bool ShowSecretKey { get; init; }
    public string? SecretKeyBase64 { get; init; }
    public bool IsAuthenticated { get; init; }
    public AuthResponse? CurrentUser { get; init; }
}
