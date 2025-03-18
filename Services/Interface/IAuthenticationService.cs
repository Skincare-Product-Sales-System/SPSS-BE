using BusinessObjects.Dto.Authentication;


namespace Services.Interface;

public interface IAuthenticationService
{
    Task<AuthenticationResponse> LoginAsync(LoginRequest loginRequest);
    Task<TokenResponse> RefreshTokenAsync(string accessToken, string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<string> RegisterAsync(RegisterRequest registerRequest);
    Task<string> RegisterForManagerAsync(RegisterRequest registerRequest);
    Task<string> RegisterForStaffAsync(RegisterRequest registerRequest);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}