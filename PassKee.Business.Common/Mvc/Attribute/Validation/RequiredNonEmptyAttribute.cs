using System.ComponentModel.DataAnnotations;

namespace PassKee.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class RequiredNonEmptyAttribute : RequiredAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return false;
            }

            // Check for empty Guid
            if (value is Guid guidValue)
            {
                return guidValue != Guid.Empty;
            }

            // Check for empty string
            if (value is string stringValue)
            {
                return !string.IsNullOrWhiteSpace(stringValue);
            }

            // Check for default values of other types (e.g. 0 for numeric types) 
            // but only if it's explicitly desired. 
            // For now, let's stick to Guid and String as they are the most common "empty" cases.
            
            return base.IsValid(value);
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (IsValid(value))
            {
                return ValidationResult.Success;
            }

            var errorMessage = ErrorMessage ?? $"The {validationContext.DisplayName} field is required and cannot be empty.";
            return new ValidationResult(errorMessage, new[] { validationContext.MemberName! });
        }
    }
}
