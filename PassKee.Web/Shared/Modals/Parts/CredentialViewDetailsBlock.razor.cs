using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;

namespace PassKee.Web.Shared.Modals.Parts;

public partial class CredentialViewDetailsBlock : ComponentBase
{
    [Parameter] public CredentialType Type { get; set; } = CredentialType.Login;
    [Parameter] public BaseCredentialPayload Payload { get; set; } = null!;
    [Parameter] public bool ShowFiles { get; set; } = true;

    protected string CardTitle => Type switch
    {
        CredentialType.Card => "Card details",
        CredentialType.SshKey => "Private key",
        _ => "Credentials"
    };

    protected string? Username => Payload switch
    {
        LoginCredentialPayload login => login.Username,
        PasswordCredentialPayload pass => pass.Username,
        ServerCredentialPayload server => server.Username,
        _ => null
    };

    protected string? Password => Payload switch
    {
        LoginCredentialPayload login => login.Password,
        PasswordCredentialPayload pass => pass.Password,
        ServerCredentialPayload server => server.Password,
        _ => null
    };

    protected string? Website => Payload switch
    {
        LoginCredentialPayload login => login.Website,
        ServerCredentialPayload server => server.Url,
        _ => null
    };

    protected string? CardholderName => Payload is CardCredentialPayload card ? card.CardholderName : null;
    protected string? CardNumber => Payload is CardCredentialPayload card ? card.CardNumber : null;
    protected string? ExpirationDate => Payload is CardCredentialPayload card ? card.ExpirationDate : null;
    protected string? Cvv => Payload is CardCredentialPayload card ? card.Cvv : null;

    protected StoredFileDto? File => Payload switch
    {
        FileCredentialPayload filePayload => filePayload.File,
        SshKeyCredentialPayload sshKey => sshKey.PrivateKeyFile,
        _ => null
    };

    protected List<CredentialField> AdditionalFields => Payload?.AdditionalFields ?? [];
    protected List<CredentialSection> Sections => Payload?.Sections ?? [];
    protected string? Notes => Payload?.Notes;
}

