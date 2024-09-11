using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AttendEase.Filters
{
    public class RoleCheckAttribute : ActionFilterAttribute
    {
        private readonly string _requiredRole;

        public RoleCheckAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userRole = session.GetString("UserRole");

            // If the user role doesn't match the required role, redirect to a "forbidden" or "unauthorized" page
            if (userRole == null || userRole != _requiredRole)
            {
                context.Result = new RedirectToActionResult("Forbidden", "Home", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
