﻿using System.ComponentModel.DataAnnotations;

namespace Shared.CustomValidations {
    public class MinimumValueAttribute(int minValue) : ValidationAttribute {
        private readonly int _minValue = minValue;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
            if (value is int intValue && intValue < _minValue) {
                return new ValidationResult($"The value of {validationContext.DisplayName} must be at least {_minValue}.");
            }

            return ValidationResult.Success;
        }
    }
}