using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Resources;
using PassKee.Business.Common.Utils;

namespace PassKee.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsBase64Attribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            if (value is string base64)
            {
                if (!Base64Utils.IsValidBase64(base64))
                {
                    return new ValidationResult(String.Format(RG.Error_IncorrectBase64, validationContext.DisplayName));    
                }

                return ValidationResult.Success;
            }

            return new ValidationResult(String.Format(RG.Error_FieldContainsIncorrectIPv4Address, validationContext.DisplayName));
        }
    }
}
