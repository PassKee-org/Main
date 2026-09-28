using System;

namespace PassKee.Web.Models.Vaults;

public record DecryptedDirectory(
    Guid Id,
    Guid VaultId,
    Guid? ParentDirectoryId,
    string Name
);
