using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Shared.CustomValidations {
    public partial class UsernameOrEmailAttribute : ValidationAttribute {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
            if (value is not string str) {
                return new ValidationResult("Invalid input.");
            }

            // Check if input is an email
            if (EmailRegex().IsMatch(str)) {
                if (str.Length > 100) {
                    return new ValidationResult("Maximum length for the Email is 100 characters.");
                }
            }
            else { // Assume input is a username
                if (str.Length > 20) {
                    return new ValidationResult("Maximum length for the Username is 20 characters.");
                }
            }

            return ValidationResult.Success;
        }

        [GeneratedRegex(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$")]
        private static partial Regex EmailRegex();
    }
}
