using BusinessObjects.Dto.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace API.Controllers;

[Route("api/authentications")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthenticationController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            var response = await _authService.LoginAsync(loginRequest);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshRequest)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(
                refreshRequest.AccessToken,
                refreshRequest.RefreshToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(401, new { message = ex.Message });
        }
    }

    // [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest logoutRequest)
    {
        try
        {
            await _authService.LogoutAsync(logoutRequest.RefreshToken);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
    [HttpPost("register")]
    public async Task<ActionResult<string>> Register(RegisterRequest registerRequest)
    {
        try
        {
            var result = await _authService.RegisterAsync(registerRequest);
            return Ok(new { message = "Registered successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}