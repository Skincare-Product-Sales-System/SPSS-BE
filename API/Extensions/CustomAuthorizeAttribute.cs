using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _requiredRoles;

        public CustomAuthorizeAttribute(params string[] requiredRoles)
        {
            _requiredRoles = requiredRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Items["Role"]?.ToString();

            // Handle multiple roles in "Role"
            if (role != null)
            {
                var userRoles = role.Split(',').Select(r => r.Trim()).ToList(); // Split and trim roles
                if (_requiredRoles.Any(requiredRole => userRoles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase)))
                {
                    return; // Authorized if any required role matches
                }
            }

            context.Result = new ForbidResult(); // Forbid if no roles match
        }
    }
}
