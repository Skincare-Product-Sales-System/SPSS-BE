using BusinessObjects.Dto.Authentication;


namespace Services.Interface;

public interface IAuthenticationService
{
    Task<AuthenticationResponse> LoginAsync(LoginRequest loginRequest);
    Task<TokenResponse> RefreshTokenAsync(string accessToken, string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<string> RegisterAsync(RegisterRequest registerRequest);
}