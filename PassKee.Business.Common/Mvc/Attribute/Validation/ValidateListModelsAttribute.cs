using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace PassKee.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateListModelsAttribute : ValidationAttribute
    {
        public ValidateListModelsAttribute()
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IEnumerable arrayToValidate)
            {
                return ValidationResult.Success;
            }

            foreach (var item in arrayToValidate)
            {
                var context = new ValidationContext(item, null, null);
                var results = new List<ValidationResult>();

                var isValid = Validator.TryValidateObject(item, context, results, true);
                if (!isValid)
                {
                    var error = results.FirstOrDefault();
                    if (error == null)
                    {
                        return ValidationResult.Success;
                    }
                    return new ValidationResult(error.ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }
}
