using System.Collections.Generic;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class VaultDetailsResponse : IResponse
{
    public VaultDto Vault { get; set; } = null!;
    public List<DirectoryDto> Directories { get; set; } = new List<DirectoryDto>();
    public List<CredentialDto> Credentials { get; set; } = new List<CredentialDto>();
}
