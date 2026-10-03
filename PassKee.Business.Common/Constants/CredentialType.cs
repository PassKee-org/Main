using System.ComponentModel;

namespace PassKee.Business.Common.Constants;

public enum CredentialType
{
    [Description("Login")]
    Login = 1,

    [Description("Secure Note")]
    SecureNote = 2,

    [Description("Card")]
    Card = 3,

    [Description("Password")]
    Password = 4,

    [Description("File")]
    File = 5
}
