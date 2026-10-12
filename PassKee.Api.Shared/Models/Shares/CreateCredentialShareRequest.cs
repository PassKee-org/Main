using System;
using Api.Requests.Abstractions;
using PassKee.Business.Common.Constants;

namespace PassKee.Api.Shared.Models.Shares;

public class CreateCredentialShareRequest : IRequest<CreateCredentialShareResponse>
{
    public Guid? CredentialId { get; set; }
    public CredentialType Type { get; set; }
    public byte[] EncryptedBody { get; set; } = null!;
    public int? ExpiresInSeconds { get; set; }
    public int? MaxViews { get; set; }
}

