using PassKee.Api.Shared.Models.Storage;

namespace PassKee.Api.Shared.Models.Vaults.Payloads;

public class SshKeyCredentialPayload : BaseCredentialPayload
{
    public StoredFileDto? PrivateKeyFile { get; set; }
}
