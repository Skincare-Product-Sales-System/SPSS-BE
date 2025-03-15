using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public CustomAuthorizeAttribute(string roles)
    {
        if (string.IsNullOrWhiteSpace(roles))
            throw new ArgumentException("Roles cannot be null or empty.", nameof(roles));

        _roles = roles.Split(",").Select(role => role.Trim()).ToArray();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check if the user is authenticated
        var user = context.HttpContext.User;
        if (user == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check if the user has any of the specified roles
        if (!_roles.Any(role => user.IsInRole(role)))
        {
            context.Result = new ForbidResult(); // Forbidden if user doesn't have the required role
        }
    }
}
