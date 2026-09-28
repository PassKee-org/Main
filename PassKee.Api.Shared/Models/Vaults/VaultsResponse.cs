using System.Collections.Generic;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class VaultsResponse : IResponse
{
    public List<VaultDto> Vaults { get; set; } = new List<VaultDto>();
}
