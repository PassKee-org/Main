using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Vaults;

public class UpdateCredentialPositionsRequest : IRequest
{
    [Required]
    public Guid VaultId { get; set; }

    public IDictionary<Guid, int> Items { get; set; } = new Dictionary<Guid, int>();
}

