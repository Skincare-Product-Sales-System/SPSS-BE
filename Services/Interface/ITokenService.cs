using BusinessObjects.Models;

namespace Services.Interface;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token, out Guid userId);
    Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string accessToken, string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}