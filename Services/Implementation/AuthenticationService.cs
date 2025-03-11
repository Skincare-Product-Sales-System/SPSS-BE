using AutoMapper;
using BusinessObjects.Dto.Authentication;
using BusinessObjects.Dto.User;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Services.Implementation
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<User> _passwordHasher;  // Changed to User instead of UserDto
        private readonly IMapper _mapper;

        public AuthenticationService(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IPasswordHasher<User> passwordHasher,  // Changed to User
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<AuthenticationResponse> LoginAsync(LoginRequest loginRequest)
        {
            // Find user entity - not the DTO
            User user = await _unitOfWork.Users.GetByEmailAsync(loginRequest.Email);
            
            if (user == null || user.IsDeleted)
                throw new UnauthorizedAccessException("Invalid credentials");
            
            // Verify password with User entity
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, loginRequest.Password);
            
            if (passwordVerificationResult != PasswordVerificationResult.Success)
                throw new UnauthorizedAccessException("Invalid credentials");
            
            // Generate tokens based on the User entity
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            // Store refresh token in database
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.UserId,
                ExpiryDate = DateTime.UtcNow.AddDays(7), // Set expiry to 7 days
                CreatedTime = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                IsDeleted = false
            };

            _unitOfWork.RefreshTokens.Add(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();
            
            // Create response
            return new AuthenticationResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.EmailAddress,
                RoleId = user.RoleId,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
        {
            // Validate the refresh token from database
            var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshTokenRequest.RefreshToken);
            
            if (storedToken == null || storedToken.IsDeleted || storedToken.ExpiryDate < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }
            
            // Get the user from database
            var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId);
            
            if (user == null || user.IsDeleted)
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }
            
            // Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            // Invalidate old refresh token
            storedToken.IsDeleted = true;
            storedToken.DeletedTime = DateTimeOffset.UtcNow;
            storedToken.DeletedBy = "System";
            
            // Save new refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.UserId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedTime = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                IsDeleted = false
            };
            
            _unitOfWork.RefreshTokens.Update(storedToken);
            _unitOfWork.RefreshTokens.Add(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();
            
            // Create response
            return new AuthenticationResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.EmailAddress,
                RoleId = user.RoleId,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        
        public async Task LogoutAsync(int userId)
        {
            // Invalidate all refresh tokens for the user
            await _unitOfWork.RefreshTokens.InvalidateUserTokensAsync(userId);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}