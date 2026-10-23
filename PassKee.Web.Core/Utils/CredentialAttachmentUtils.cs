using System;
using System.Collections.Generic;
using System.Linq;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Core.Utils;

public static class CredentialAttachmentUtils
{
    /// <summary>
    /// Collects all unique attachment file IDs from a credential payload.
    /// </summary>
    public static List<Guid> CollectAttachmentFileIds(BaseCredentialPayload? payload)
    {
        if (payload == null)
        {
            return [];
        }

        var fileIds = new HashSet<Guid>();

        void AddFile(StoredFileDto? file)
        {
            if (file != null && file.Id != Guid.Empty)
            {
                fileIds.Add(file.Id);
            }
        }

        switch (payload)
        {
            case FileCredentialPayload filePayload:
                AddFile(filePayload.File);
                break;
            case SshKeyCredentialPayload sshPayload:
                AddFile(sshPayload.PrivateKeyFile);
                break;
        }

        if (payload.AdditionalFields != null)
        {
            foreach (var field in payload.AdditionalFields)
            {
                if (field.Type == CredentialFieldType.File)
                {
                    AddFile(field.File);
                }
            }
        }

        if (payload.Sections != null)
        {
            foreach (var section in payload.Sections)
            {
                if (section.Fields != null)
                {
                    foreach (var field in section.Fields)
                    {
                        if (field.Type == CredentialFieldType.File)
                        {
                            AddFile(field.File);
                        }
                    }
                }
            }
        }

        return fileIds.ToList();
    }

    /// <summary>
    /// Identifies all file IDs from the deleting credentials that are NOT referenced in any remaining credential.
    /// </summary>
    public static List<Guid> FindOrphanFileIds(
        IEnumerable<DecryptedCredential> deletingCredentials,
        IEnumerable<DecryptedCredential> allKnownCredentials)
    {
        var deletingList = deletingCredentials.ToList();
        if (deletingList.Count == 0)
        {
            return [];
        }

        var deletingIds = deletingList.Select(c => c.Id).ToHashSet();
        var deletingFileIds = deletingList
            .SelectMany(c => CollectAttachmentFileIds(c.Payload))
            .ToHashSet();

        if (deletingFileIds.Count == 0)
        {
            return [];
        }

        var remainingFileIds = allKnownCredentials
            .Where(c => !deletingIds.Contains(c.Id))
            .SelectMany(c => CollectAttachmentFileIds(c.Payload))
            .ToHashSet();

        return deletingFileIds.Where(fid => !remainingFileIds.Contains(fid)).ToList();
    }
}

