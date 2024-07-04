using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace ChatRoom.UnitTest.Helpers {
    public static class ModelValidator {
        public static void ValidateModel(object model, ControllerBase controller, ActionExecutingContext context) {
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, true);

            foreach (var validationResult in validationResults) {
                var memberName = validationResult.MemberNames.First();
                var errorMessage = validationResult.ErrorMessage ?? $"Property has invalid value.";
                controller.ModelState.AddModelError(memberName, errorMessage);
                context.ModelState.AddModelError(memberName, errorMessage);
            }
        }
    }
}
