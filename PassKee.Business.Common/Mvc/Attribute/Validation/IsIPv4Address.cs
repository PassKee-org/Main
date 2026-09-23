using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Resources;

namespace PassKee.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsIPv4Address : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            if (value is string ipAddress)
            {
                var quads = ipAddress.Split('.');

                // if we do not have 4 quads, return false
                if (!(quads.Length == 4))
                {
                    return new ValidationResult(String.Format(RG.Error_FieldContainsIncorrectIPv4Address, validationContext.DisplayName));
                }

                // for each quad
                foreach (var quad in quads)
                {
                    int q;
                    // if parse fails 
                    // or length of parsed int != length of quad string (i.e.; '1' vs '001')
                    // or parsed int < 0
                    // or parsed int > 255
                    // return false
                    if (!Int32.TryParse(quad, out q)
                        || !q.ToString().Length.Equals(quad.Length)
                        || q < 0
                        || q > 255) {
                        return new ValidationResult(String.Format(RG.Error_FieldContainsIncorrectIPv4Address, validationContext.DisplayName));
                    }

                }
                return ValidationResult.Success;
            }

            return new ValidationResult(String.Format(RG.Error_FieldContainsIncorrectIPv4Address, validationContext.DisplayName));
        }
    }
}
