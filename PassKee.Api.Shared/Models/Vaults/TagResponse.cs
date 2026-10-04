using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class TagResponse : IResponse
{
    public TagDto Tag { get; set; } = null!;
}
