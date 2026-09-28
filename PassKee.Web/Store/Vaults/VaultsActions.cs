using System;
using System.Collections.Generic;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Store.Vaults;

public record LoadVaultsAction;
public record LoadVaultsSuccessAction(List<VaultDto> Vaults);
public record LoadVaultsFailureAction;

public record SelectVaultAction(Guid VaultId);

public record LoadVaultDetailsAction(Guid VaultId);
public record LoadVaultDetailsSuccessAction(
    Guid VaultId,
    byte[] ActiveVaultKey,
    List<DecryptedDirectory> Directories, 
    List<DecryptedCredential> Credentials
);
public record LoadVaultDetailsFailureAction;

// Vault Management
public record CreateVaultAction(string Name, string Description);
public record ResetVaultsStateAction;

// Directory CRUD
public record CreateDirectoryAction(Guid VaultId, Guid? ParentId, string Name);
public record UpdateDirectoryAction(Guid VaultId, Guid DirectoryId, Guid? ParentId, string Name);
public record DeleteDirectoryAction(Guid VaultId, Guid DirectoryId);

// Credential CRUD
public record CreateCredentialAction(Guid VaultId, Guid? DirectoryId, PassKee.Api.Shared.Models.Vaults.Enums.CredentialType Type, PassKee.Api.Shared.Models.Vaults.Payloads.BaseCredentialPayload Payload);
public record UpdateCredentialAction(Guid VaultId, Guid CredentialId, Guid? DirectoryId, PassKee.Api.Shared.Models.Vaults.Enums.CredentialType Type, PassKee.Api.Shared.Models.Vaults.Payloads.BaseCredentialPayload Payload);
public record DeleteCredentialAction(Guid VaultId, Guid CredentialId);
