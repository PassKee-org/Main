using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DeleteVaultRequest : IRequest<DeleteVaultResponse>
{
    public Guid VaultId { get; set; }
}

