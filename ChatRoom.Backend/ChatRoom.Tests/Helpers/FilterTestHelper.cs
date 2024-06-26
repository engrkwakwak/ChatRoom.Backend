using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace ChatRoom.UnitTest.Helpers {
    public static class FilterTestHelper {
        public static ActionExecutingContext CreateActionExecutingContext(ControllerBase controller, IDictionary<string, object> actionArguments, IFilterMetadata? filter = null) {

            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            return new ActionExecutingContext(
                actionContext,
                filter == null ? new List<IFilterMetadata>() : [filter],
                actionArguments!,
                controller
            );
        }

        public static void InvokeActionFilter(IActionFilter filter, ActionExecutingContext context) {
            filter.OnActionExecuting(context);
        }
    }
}
