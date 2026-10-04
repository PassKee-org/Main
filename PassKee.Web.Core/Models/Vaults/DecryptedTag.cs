using System;

namespace PassKee.Web.Models.Vaults;

public record DecryptedTag(
    Guid Id,
    Guid VaultId,
    string Name
);
