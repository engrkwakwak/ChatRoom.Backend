using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChatRoom.Backend.Presentation.ActionFilters {
    public class ValidationFilterAttribute : IActionFilter {
        public void OnActionExecuted(ActionExecutedContext context) {
        }

        public void OnActionExecuting(ActionExecutingContext context) {
            var action = context.RouteData.Values["action"];
            var controller = context.RouteData.Values["controller"];

            // Validate integer parameters
            foreach (var argument in context.ActionArguments) {
                if (argument.Value is int intValue) {
                    if (intValue < 1) {
                        context.Result = new BadRequestObjectResult($"{argument.Key} must be greater than zero. Controller: {controller}, action: {action}");
                        return;
                    }
                }
            }

            // Validate DTO parameters if present
            var param = context.ActionArguments.SingleOrDefault(x => x.Value?.ToString()?.Contains("Dto") == true).Value;
            if (param == null && context.ActionArguments.Any(x => x.Key.Contains("Dto"))) {
                context.Result = new BadRequestObjectResult($"Object is null. Controller: {controller}, action: {action}");
                return;
            }

            if (!context.ModelState.IsValid) {
                context.Result = new UnprocessableEntityObjectResult(context.ModelState);
            }
        }
    }
}
