using System;
using System.Collections.Generic;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Business.Common.Constants;
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
    List<DecryptedCredential> Credentials,
    List<DecryptedTag> Tags
);
public record LoadVaultDetailsFailureAction;

// Tag Actions
public record CreateTagAction(Guid VaultId, string Name, TaskCompletionSource<DecryptedTag?>? Completion = null);
public record CreateTagSuccessAction(DecryptedTag Tag);
public record UpdateTagAction(Guid TagId, string Name, TaskCompletionSource<DecryptedTag?>? Completion = null);
public record UpdateTagSuccessAction(DecryptedTag Tag);

// Vault Management
public record CreateVaultAction(string Name, string Description);
public record CreateVaultSuccessAction(VaultDto Vault);
public record CreateVaultFailureAction;
public record ResetVaultsStateAction;

// Directory CRUD
public record SelectDirectoryAction(Guid? DirectoryId);
public record CreateDirectoryAction(Guid VaultId, Guid? ParentId, string Name);
public record CreateDirectorySuccessAction(DecryptedDirectory Directory);
public record UpdateDirectoryAction(Guid VaultId, Guid DirectoryId, Guid? ParentId, string Name);
public record UpdateDirectorySuccessAction(DecryptedDirectory Directory);
public record DeleteDirectoryAction(Guid VaultId, Guid DirectoryId);
public record DeleteDirectorySuccessAction(Guid VaultId, Guid DirectoryId, List<Guid>? DeletedDirectoryIds = null, List<Guid>? DeletedCredentialIds = null);

// Credential CRUD
public record CredentialSaveResult(Guid RequestId, Guid? VaultId, Guid? CredentialId, Guid? DirectoryId, string? ErrorMessage)
{
    public bool IsSuccess => CredentialId.HasValue;
}

public record CreateCredentialAction(Guid RequestId, CredentialType Type, PassKee.Api.Shared.Models.Vaults.Payloads.BaseCredentialPayload Payload, TaskCompletionSource<CredentialSaveResult> Completion, Guid? DirectoryId = null);
public record CreateCredentialSuccessAction(DecryptedCredential Credential);
public record UpdateCredentialAction(Guid RequestId, Guid CredentialId, Guid? DirectoryId, CredentialType Type, PassKee.Api.Shared.Models.Vaults.Payloads.BaseCredentialPayload Payload, TaskCompletionSource<CredentialSaveResult> Completion);
public record UpdateCredentialSuccessAction(DecryptedCredential Credential);
public record DeleteCredentialAction(Guid VaultId, Guid CredentialId);
public record DeleteCredentialSuccessAction(Guid VaultId, Guid CredentialId);
public record ArchiveCredentialAction(Guid VaultId, Guid CredentialId);
public record ArchiveCredentialSuccessAction(Guid VaultId, Guid CredentialId, DecryptedCredential? Credential = null);

// Archive Navigation & Loading
public record SelectArchiveAction;
public record LoadArchivedCredentialsAction(Guid VaultId);
public record LoadArchivedCredentialsSuccessAction(Guid VaultId, List<DecryptedCredential> Credentials);
public record LoadArchivedCredentialsFailureAction;

// File upload (independent of credential save: an uploaded file is attached to a credential only when the credential is saved)
public record FileUploadResult(Guid RequestId, PassKee.Api.Shared.Models.Storage.StoredFileDto? File, string? ErrorMessage)
{
    public bool IsSuccess => File != null;
}

public record UploadVaultFileAction(Guid RequestId, Microsoft.AspNetCore.Components.Forms.IBrowserFile BrowserFile, TaskCompletionSource<FileUploadResult> Completion);
public record UploadVaultFileFinishedAction(Guid RequestId);

// Search Actions
public record SetSearchQueryAction(string Query);
public record ClearSearchQueryAction;

