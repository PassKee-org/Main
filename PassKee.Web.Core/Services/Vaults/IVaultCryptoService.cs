using System.Collections.Generic;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Api.Shared.Models.Vaults.Enums;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Core.Services.Vaults;

public interface IVaultCryptoService
{
    byte[] EncryptVaultKey(byte[] vaultKey, byte[] userPublicKey);
    byte[] DecryptVaultKey(byte[] encryptedVaultKey, byte[] userPrivateKey);
    byte[] EncryptDirectoryName(string name, byte[] vaultKey);
    string DecryptDirectoryName(byte[] encryptedName, byte[] vaultKey);
    List<DecryptedDirectory> DecryptDirectories(IEnumerable<DirectoryDto> directories, byte[] vaultKey);
    byte[] EncryptCredentialPayload(BaseCredentialPayload payload, byte[] vaultKey);
    BaseCredentialPayload DecryptCredentialPayload(byte[] encryptedBody, CredentialType type, byte[] vaultKey);
    List<DecryptedCredential> DecryptCredentials(IEnumerable<CredentialDto> credentials, byte[] vaultKey);
}
