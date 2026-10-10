using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class GetArchivedCredentialsRequest : IRequest<ArchivedCredentialsResponse>
{
    public Guid VaultId { get; set; }
}

