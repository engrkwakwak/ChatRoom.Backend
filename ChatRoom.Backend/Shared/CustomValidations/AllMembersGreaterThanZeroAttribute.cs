using System.ComponentModel.DataAnnotations;

namespace Shared.CustomValidations {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class AllMembersGreaterThanZeroAttribute : ValidationAttribute {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
            if (value is IEnumerable<int> memberIds) {
                if (memberIds.Any(id => id < 1)) {
                    return new ValidationResult("All member IDs must be greater than or equal to 1.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
