using System;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Helpers;

namespace PassKee.Web.Core.Utils;

public static class CredentialCloner
{
    public static BaseCredentialPayload CloneWithCopyTitle(BaseCredentialPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var json = JsonHelper.SerializeToString(payload);
        var clone = (BaseCredentialPayload)JsonHelper.DeserializeObject(json, payload.GetType())!;
        var originalTitle = clone.Title?.Trim() ?? string.Empty;
        clone.Title = string.IsNullOrWhiteSpace(originalTitle) ? "Copy" : $"{originalTitle} Copy";
        return clone;
    }
}

