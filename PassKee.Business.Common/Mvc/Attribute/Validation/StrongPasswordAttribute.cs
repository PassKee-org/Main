using System;
using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Utils;
using PassKee.Business.Common.Utils.Validators;

namespace PassKee.Business.Common.Mvc.Attribute.Validation;

/// <summary>
/// Validates that a password satisfies complexity requirements (min length, digits, Latin upper and lower case).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class StrongPasswordAttribute : ValidationAttribute
{
    public int MinLength { get; set; } = PasswordValidator.DefaultMinLength;

    public StrongPasswordAttribute()
    {
    }

    public StrongPasswordAttribute(int minLength)
    {
        MinLength = minLength;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Password is required.");
        }

        if (value is string password)
        {
            var result = PasswordValidator.Validate(password, MinLength);
            if (!result.IsValid)
            {
                return new ValidationResult(result.ErrorMessage);
            }

            return ValidationResult.Success;
        }

        return new ValidationResult("Invalid password format.");
    }
}
