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
    public List<VaultDto> Vaults { get; init; } = new();
    public Guid? ActiveVaultId { get; init; }
    public byte[]? ActiveVaultKey { get; init; }
    
    // Details for the active vault
    public List<DecryptedDirectory> Directories { get; init; } = new();
    public List<DecryptedCredential> Credentials { get; init; } = new();

    public VaultsState() {} // Required for Fluxor
    public VaultsState(bool isLoading, List<VaultDto> vaults, Guid? activeVaultId, byte[]? activeVaultKey, List<DecryptedDirectory> directories, List<DecryptedCredential> credentials)
    {
        IsLoading = isLoading;
        Vaults = vaults;
        ActiveVaultId = activeVaultId;
        ActiveVaultKey = activeVaultKey;
        Directories = directories;
        Credentials = credentials;
    }
}
