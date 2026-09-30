namespace PassKee.Web.Core.Ui.Shared.Components.Enums;

/// <summary>
/// Specifies the type of secret handled by SecretInput.
/// </summary>
public enum SecretInputType
{
    /// <summary>
    /// Master password or account password with strong password generator.
    /// </summary>
    Password,

    /// <summary>
    /// High-entropy emergency Secret Key with string generator and copy support.
    /// </summary>
    SecretKey,

    /// <summary>
    /// Generic secret string or token.
    /// </summary>
    Custom
}
