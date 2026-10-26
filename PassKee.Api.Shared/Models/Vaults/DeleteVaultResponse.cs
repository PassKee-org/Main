using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class DeleteVaultResponse : IResponse
{
    public Guid VaultId { get; set; }
}

