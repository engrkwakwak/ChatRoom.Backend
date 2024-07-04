using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace ChatRoom.UnitTest.Helpers {
    public static class FilterTestHelper {
        public static ActionExecutingContext CreateActionExecutingContext(
            ControllerBase controller,
            string actionName,
            string controllerName,
            IDictionary<string, object> actionArguments,
            IFilterMetadata? filter = null
        ) {
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            routeData.Values["action"] = actionName;
            routeData.Values["controller"] = controllerName;

            var actionDescriptor = new ActionDescriptor {
                RouteValues = {
                    ["action"] = actionName,
                    ["controller"] = controllerName
                }
            };

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var filters = filter == null ? [] : new List<IFilterMetadata> { filter };

            return new ActionExecutingContext(
                actionContext,
                filters,
                actionArguments!,
                controller
            );
        }

        public static void InvokeActionFilter(IActionFilter filter, ActionExecutingContext context) {
            filter.OnActionExecuting(context);
        }
    }
}
