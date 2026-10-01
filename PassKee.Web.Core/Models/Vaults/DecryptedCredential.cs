using System;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;

namespace PassKee.Web.Models.Vaults;

public record DecryptedCredential(
    Guid Id,
    Guid VaultId,
    Guid? DirectoryId,
    CredentialType Type,
    BaseCredentialPayload Payload
);

