using System.Security.Claims;
using BusinessObjects.Dto.User;
using BusinessObjects.Models;

namespace Services.Interface;

public interface ITokenService
{
    string GenerateAccessToken(UserDto user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}