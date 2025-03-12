using AutoMapper;
using BusinessObjects.Dto.Authentication;
using BusinessObjects.Dto.User;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;

namespace Services.Implementation;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthenticationService(IUnitOfWork unitOfWork, ITokenService tokenService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<AuthenticationResponse> LoginAsync(LoginRequest loginRequest)
    {
        // Find user by username
        var user = await _unitOfWork.Users.GetByUserNameAsync(loginRequest.UserName);
        
        if (user == null || user.IsDeleted)
            throw new UnauthorizedAccessException("Invalid username or password");

        // Verify password
        // Note: In production, use a proper password hashing library like BCrypt
        if (user.Password != loginRequest.Password)
            throw new UnauthorizedAccessException("Invalid username or password");

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.UserId,
            ExpiryTime = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            IsRevoked = false,
            IsUsed = false
        };
        
        _unitOfWork.RefreshTokens.Add(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();
        
        // Map user to UserDto
        var userDto = _mapper.Map<UserDto>(user);
        
        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = userDto
        };
    }

    public async Task<TokenResponse> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var (newAccessToken, newRefreshToken) = await _tokenService.RefreshTokenAsync(accessToken, refreshToken);
        
        return new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
    }
}