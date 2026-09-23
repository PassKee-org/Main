using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Resources;

namespace PassKee.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsPositiveAttribute : ValidationAttribute
    {
        public bool AllowZero = false;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            if (value is int)
            {
                int getal;
                if (int.TryParse(value.ToString(), out getal))
                {
                    if(AllowZero && getal >= 0)
                        return ValidationResult.Success;

                    if (getal > 0)
                        return ValidationResult.Success;
                }
            }
            if (value is long)
            {
                long getal;
                if (long.TryParse(value.ToString(), out getal))
                {
                    if (AllowZero && getal >= 0)
                        return ValidationResult.Success;

                    if (getal > 0)
                        return ValidationResult.Success;
                }
            }
            else if (value is decimal)
            {
                decimal getal;
                if (decimal.TryParse(value.ToString(), out getal))
                {
                    if (AllowZero && getal >= 0m)
                        return ValidationResult.Success;

                    if (getal > 0m)
                        return ValidationResult.Success;
                }
            }
            else if (value is float)
            {
                float getal;
                if (float.TryParse(value.ToString(), out getal))
                {
                    if (AllowZero && getal >= 0f)
                        return ValidationResult.Success;

                    if (getal > 0f)
                        return ValidationResult.Success;
                }
            }
            else if (value is double)
            {
                double getal;
                if (double.TryParse(value.ToString(), out getal))
                {
                    if (AllowZero && getal >= 0d)
                        return ValidationResult.Success;

                    if (getal > 0d)
                        return ValidationResult.Success;
                }
            }

            ErrorMessage ??= string.Format(RG.Error_FieldMayContainOnlyPositiveDigits, validationContext.DisplayName);
            return new ValidationResult(ErrorMessage);
        }
    }
}
