using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace API.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public AuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var allowAnonymousRefreshTokenAttribute = endpoint.Metadata.GetMetadata<AllowAnonymousRefreshTokenAttribute>();
            if (allowAnonymousRefreshTokenAttribute != null)
            {
                await _next(context);
                return;
            }
        }

        if (context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader) && !string.IsNullOrEmpty(authorizationHeader))
        {
            var token = authorizationHeader.ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);

                    // Validate token expiration
                    if (jwtToken.ValidTo < DateTime.UtcNow)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        await context.Response.WriteAsync("Token has expired");
                        return;
                    }

                    // Extract claims
                    var claims = jwtToken.Claims.ToList();

                    // Extract and store individual claims in HttpContext.Items
                    var userIdClaim = claims.FirstOrDefault(c => c.Type == "Id")?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
                    {
                        context.Items["UserId"] = userId;
                    }

                    var userNameClaim = claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
                    if (!string.IsNullOrEmpty(userNameClaim))
                    {
                        context.Items["UserName"] = userNameClaim;
                    }

                    var emailClaim = claims.FirstOrDefault(c => c.Type == "Email")?.Value;
                    if (!string.IsNullOrEmpty(emailClaim))
                    {
                        context.Items["Email"] = emailClaim;
                    }

                    var avatarUrlClaim = claims.FirstOrDefault(c => c.Type == "AvatarUrl")?.Value;
                    if (!string.IsNullOrEmpty(avatarUrlClaim))
                    {
                        context.Items["AvatarUrl"] = avatarUrlClaim;
                    }

                    var roleClaim = claims.FirstOrDefault(c => c.Type == "Role")?.Value;
                    if (!string.IsNullOrEmpty(roleClaim))
                    {
                        context.Items["Role"] = roleClaim;
                    }
                }
                catch (SecurityTokenException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Invalid token");
                    return;
                }
            }
        }

        await _next(context);
    }
}
