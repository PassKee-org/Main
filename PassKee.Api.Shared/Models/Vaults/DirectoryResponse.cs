using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DirectoryResponse : IResponse
{
    public DirectoryDto Directory { get; set; } = null!;
}
