using System;
using PassKee.Api.Shared.Models.Vaults.Payloads;

namespace PassKee.Web.Models.Vaults;

public record DecryptedCredential(
    Guid Id,
    Guid VaultId,
    Guid? DirectoryId,
    PassKee.Api.Shared.Models.Vaults.Enums.CredentialType Type,
    BaseCredentialPayload Payload
);
