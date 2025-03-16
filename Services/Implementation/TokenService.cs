using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BusinessObjects.Dto.Authentication;
using BusinessObjects.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Interface;
using Services.Interface;

namespace Services.Implementation;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeSpan _accessTokenExpiration;
    private readonly TimeSpan _refreshTokenExpiration;

    public TokenService(IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _accessTokenExpiration = TimeSpan.FromDays(double.Parse(_configuration["Jwt:AccessTokenExpirationDays"] ?? "30"));
        _refreshTokenExpiration = TimeSpan.FromDays(double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));
    }

    public async Task<string> GenerateAccessTokenAsync(AuthUserDto user)
    {
        var claims = new List<Claim>
        {
            new Claim("Id", user.UserId.ToString()),
            new Claim("UserName", user.UserName),
            new Claim("Email", user.EmailAddress),
            new Claim("AvatarUrl", user.AvatarUrl ?? string.Empty),
            new Claim("Role", user.Role ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.Add(_accessTokenExpiration);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public bool ValidateAccessToken(string token, out Guid userId)
    {
        userId = Guid.Empty;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            
            if (int.TryParse(userIdClaim, out int id))
            {
                userId = Guid.Parse(userIdClaim);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        // Find the refresh token in the database
        var storedRefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
    
        if (storedRefreshToken == null)
            throw new SecurityTokenException("Invalid refresh token");
        
        if (storedRefreshToken.IsUsed || storedRefreshToken.IsRevoked)
            throw new SecurityTokenException("Refresh token has been used or revoked");
        
        if (storedRefreshToken.ExpiryTime < DateTimeOffset.UtcNow)
            throw new SecurityTokenException("Refresh token has expired");

        // Mark the current refresh token as used
        storedRefreshToken.IsUsed = true;
        _unitOfWork.RefreshTokens.Update(storedRefreshToken);
    
        // Get the user associated with the refresh token
        var user = await _unitOfWork.Users.GetByIdAsync(storedRefreshToken.UserId);
        if (user == null || user.IsDeleted)
            throw new SecurityTokenException("User not found");

        // Map the user to AuthUserDto
        var authUserDto = new AuthUserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            EmailAddress = user.EmailAddress,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role!.RoleName // Assuming Role is included in User and accessible
        };

        // Generate new tokens - add await here
        var newAccessToken = await GenerateAccessTokenAsync(authUserDto);
        var newRefreshToken = GenerateRefreshToken();
    
        // Save the new refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.UserId,
            ExpiryTime = DateTime.UtcNow.Add(_refreshTokenExpiration),
            Created = DateTime.UtcNow,
            IsRevoked = false,
            IsUsed = false
        };
    
        _unitOfWork.RefreshTokens.Add(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();
    
        return (newAccessToken, newRefreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var storedRefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        
        if (storedRefreshToken == null)
            throw new SecurityTokenException("Invalid refresh token");
            
        storedRefreshToken.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(storedRefreshToken);
        await _unitOfWork.SaveChangesAsync();
    }
}