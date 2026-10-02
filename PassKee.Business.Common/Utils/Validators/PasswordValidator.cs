using System;

namespace PassKee.Business.Common.Utils.Validators;

public record PasswordValidationResult(bool IsValid, string? ErrorMessage = null);

/// <summary>
/// Validates password complexity: enforces minimum length, digits, and Latin uppercase and lowercase letters.
/// </summary>
public static class PasswordValidator
{
    public const int DefaultMinLength = 10;
    public const int DefaultMaxLength = 128;

    public static PasswordValidationResult Validate(string? password, int minLength = DefaultMinLength)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return new PasswordValidationResult(false, "Password cannot be empty.");
        }

        if (password.Length < minLength)
        {
            return new PasswordValidationResult(false, $"Password must be at least {minLength} characters long.");
        }

        if (password.Length > DefaultMaxLength)
        {
            return new PasswordValidationResult(false, $"Password cannot exceed {DefaultMaxLength} characters.");
        }

        bool hasDigit = false;
        bool hasUpper = false;
        bool hasLower = false;

        foreach (char c in password)
        {
            if (char.IsAsciiDigit(c))
            {
                hasDigit = true;
            }
            else if (char.IsAsciiLetterUpper(c))
            {
                hasUpper = true;
            }
            else if (char.IsAsciiLetterLower(c))
            {
                hasLower = true;
            }
        }

        if (!hasDigit)
        {
            return new PasswordValidationResult(false, "Password must contain at least one digit (0-9).");
        }

        if (!hasLower)
        {
            return new PasswordValidationResult(false, "Password must contain at least one lowercase Latin letter (a-z).");
        }

        if (!hasUpper)
        {
            return new PasswordValidationResult(false, "Password must contain at least one uppercase Latin letter (A-Z).");
        }

        return new PasswordValidationResult(true);
    }

    public static bool IsValid(string? password, out string? errorMessage, int minLength = DefaultMinLength)
    {
        var result = Validate(password, minLength);
        errorMessage = result.ErrorMessage;
        return result.IsValid;
    }

    public static bool IsValid(string? password, int minLength = DefaultMinLength)
    {
        return Validate(password, minLength).IsValid;
    }
}
