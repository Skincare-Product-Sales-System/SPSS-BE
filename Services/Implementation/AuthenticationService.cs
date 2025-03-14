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
        User user = null;
        if (loginRequest.UsernameOrEmail.Contains('@')) 
            user = await _unitOfWork.Users.GetByEmailAsync(loginRequest.UsernameOrEmail);
        if (user == null)
            user = await _unitOfWork.Users.GetByUserNameAsync(loginRequest.UsernameOrEmail);
        if (user == null || user.IsDeleted)
            throw new UnauthorizedAccessException("Invalid username/email or password");
        if (user.Password != loginRequest.Password)
            throw new UnauthorizedAccessException("Invalid username/email or password");
        
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
    
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
    
        var authUserDto = new AuthUserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            EmailAddress = user.EmailAddress,
            AvatarUrl = user.AvatarUrl
        };
    
        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            // AuthUserDto = authUserDto
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