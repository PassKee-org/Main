using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class GetVaultDetailsRequest : IRequest<VaultDetailsResponse>
{
    public Guid VaultId { get; set; }
}
