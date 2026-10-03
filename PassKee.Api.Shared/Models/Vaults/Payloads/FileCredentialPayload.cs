using PassKee.Api.Shared.Models.Storage;

namespace PassKee.Api.Shared.Models.Vaults.Payloads;

public class FileCredentialPayload : BaseCredentialPayload
{
    public StoredFileDto? File { get; set; }
}
