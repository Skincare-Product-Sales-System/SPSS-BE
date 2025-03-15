using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _requiredRole;

        public CustomAuthorizeAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Items["Role"]?.ToString();
            if (role != _requiredRole)
            {
                context.Result = new ForbidResult();
            }
        }
    }

}
