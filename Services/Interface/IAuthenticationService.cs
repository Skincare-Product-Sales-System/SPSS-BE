using BusinessObjects.Dto.Authentication;

namespace Services.Interface;

public interface IAuthenticationService
{
    Task<AuthenticationResponse> LoginAsync(LoginRequest loginRequest);
    Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
    Task LogoutAsync(int userId);
}