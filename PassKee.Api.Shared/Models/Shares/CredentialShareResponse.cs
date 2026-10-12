using System;
using Api.Requests.Abstractions;
using PassKee.Business.Common.Constants;

namespace PassKee.Api.Shared.Models.Shares;

public class CredentialShareResponse : IResponse
{
    public CredentialType Type { get; set; }
    public byte[] EncryptedBody { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

