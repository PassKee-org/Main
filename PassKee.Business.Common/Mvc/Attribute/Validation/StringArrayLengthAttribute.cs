using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Resources;

namespace PassKee.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class StringArrayLengthAttribute : ValidationAttribute
    {
        public int MaximumLength { get; }
        public int MinLength { get; set; }

        public StringArrayLengthAttribute(int MaximumLength)
        {
            MinLength = 0;
            this.MaximumLength = MaximumLength;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(value is string[] arrayToValidate))
            {
                return ValidationResult.Success;
            }
            foreach (var stringToValidate in arrayToValidate)
            {
                if (stringToValidate == null)
                {
                    return new ValidationResult(
                        string.Format(
                            RG.Error_ArrayStringCanNotBeNull, 
                            validationContext.DisplayName, 
                            MaximumLength
                        )
                    );
                }
                if (stringToValidate.Length > MaximumLength)
                {
                    return new ValidationResult(
                        string.Format(
                            RG.Error_ArrayStringIsTooLong, 
                            validationContext.DisplayName, 
                            MaximumLength
                        )
                    );
                }
                if (MinLength > 0 && stringToValidate.Length < MinLength)
                {
                    return new ValidationResult(
                        string.Format(
                            RG.Error_ArrayStringIsTooShort, 
                            validationContext.DisplayName, 
                            MinLength
                        )
                    );
                }
            }
            return ValidationResult.Success;
        }
    }
}
