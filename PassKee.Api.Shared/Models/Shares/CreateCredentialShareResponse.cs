using System;
using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Models.Shares;

public class CreateCredentialShareResponse : IResponse
{
    public string Token { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
    public int? MaxViews { get; set; }
}

