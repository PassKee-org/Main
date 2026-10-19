using System;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;

namespace PassKee.Web.Models.Vaults;

public record DecryptedCredential(
    Guid Id,
    Guid VaultId,
    Guid? DirectoryId,
    CredentialType Type,
    BaseCredentialPayload Payload,
    DateTime? ArchivedAt = null,
    int Order = 0
)
{
    public DecryptedCredential(
        Guid id,
        Guid vaultId,
        Guid? directoryId,
        CredentialType type,
        BaseCredentialPayload payload,
        int order
    ) : this(id, vaultId, directoryId, type, payload, null, order)
    {
    }

    public DecryptedCredential(
        CredentialDto credential,
        BaseCredentialPayload payload
    ) : this(
        credential.Id,
        credential.VaultId,
        credential.DirectoryId,
        credential.Type,
        payload,
        credential.ArchivedAt,
        credential.Order
    )
    {
    }
}


