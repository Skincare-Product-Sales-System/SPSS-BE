using System.IdentityModel.Tokens.Jwt;
using System.Net;
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

        if (context.Request.Headers.ContainsKey("Authorization"))
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);

                    // Kiểm tra thời gian hết hạn của token
                    if (jwtToken.ValidTo < DateTime.UtcNow)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        await context.Response.WriteAsync("Token has expired");
                        return;
                    }

                    // Lấy userId từ token
                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
                    {
                        // Lưu userId vào HttpContext.Items
                        context.Items["UserId"] = userId;
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
