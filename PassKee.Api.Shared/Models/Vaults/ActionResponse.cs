using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class ActionResponse : IResponse
{
    public bool Success { get; set; }
}
