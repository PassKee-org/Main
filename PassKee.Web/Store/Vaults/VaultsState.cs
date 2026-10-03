using System;
using System.Collections.Generic;
using Fluxor;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Store.Vaults;

[FeatureState]
public record VaultsState
{
    public bool IsLoading { get; init; }
    public bool IsCreating { get; init; }
    public List<VaultDto> Vaults { get; init; } = new();
    public Guid? ActiveVaultId { get; init; }
    public byte[]? ActiveVaultKey { get; init; }
    
    // Details for the active vault
    public List<DecryptedDirectory> Directories { get; init; } = new();
    public List<DecryptedCredential> Credentials { get; init; } = new();
    public Guid? SelectedDirectoryId { get; init; }

    public int PendingFileUploads { get; init; }
}
