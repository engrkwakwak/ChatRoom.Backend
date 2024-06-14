using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Shared.CustomValidations {
    public class MinimumElementsAttribute(int minElements) : ValidationAttribute {
        private readonly int _minElements = minElements;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
            if (value is IEnumerable enumerable) {
                int count = 0;
                foreach (var item in enumerable) {
                    count++;
                }

                if (count < _minElements) {
                    return new ValidationResult($"The collection {validationContext.DisplayName} must have at least {_minElements} elements.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult($"{validationContext.DisplayName} is not a valid collection.");
        }
    }
}
