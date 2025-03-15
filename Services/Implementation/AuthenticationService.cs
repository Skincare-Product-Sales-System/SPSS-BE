using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObjects.Dto.Authentication;
using BusinessObjects.Dto.Role;
using BusinessObjects.Dto.User;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Repositories.Interface;
using Services.Interface;
using Mapster;
namespace Services.Implementation;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    public AuthenticationService(IRoleService roleService,IUserService userService, IUnitOfWork unitOfWork, ITokenService tokenService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _mapper = mapper;
        _userService = userService;
        _roleService = roleService;
    }

    public async Task<AuthenticationResponse> LoginAsync(LoginRequest loginRequest)
    {
        User user = null;

        // Lấy thông tin user dựa vào email hoặc username
        if (loginRequest.UsernameOrEmail.Contains('@'))
            user = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.Role) // Bao gồm Role
                .FirstOrDefaultAsync(u => u.EmailAddress == loginRequest.UsernameOrEmail);
        if (user == null)
            user = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.Role) // Bao gồm Role
                .FirstOrDefaultAsync(u => u.UserName == loginRequest.UsernameOrEmail);

        // Kiểm tra tính hợp lệ
        if (user == null || user.IsDeleted)
            throw new UnauthorizedAccessException("Invalid username/email or password");
        if (user.Password != loginRequest.Password)
            throw new UnauthorizedAccessException("Invalid username/email or password");
        // Map the user to AuthUserDto
        var authUserDto = new AuthUserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            EmailAddress = user.EmailAddress,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role!.RoleName // Assuming Role is included in User and accessible
        };
        // Tạo AccessToken và RefreshToken
        var accessToken = await _tokenService.GenerateAccessTokenAsync(authUserDto);
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

        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AuthUserDto = authUserDto
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
    
    public async Task<string> RegisterAsync(RegisterRequest registerRequest)
    {
        // Log giá trị của RegisterRequest
        Console.WriteLine($"UserName: {registerRequest.UserName}");
        Console.WriteLine($"SurName: {registerRequest.SurName}");
        Console.WriteLine($"LastName: {registerRequest.LastName}");
        Console.WriteLine($"EmailAddress: {registerRequest.EmailAddress}");
        Console.WriteLine($"PhoneNumber: {registerRequest.PhoneNumber}");
        Console.WriteLine($"Password: {registerRequest.Password}");
        
        
        if (await _userService.CheckUserNameExistsAsync(registerRequest.UserName))
            throw new UnauthorizedAccessException("Username đã được sử dụng");

        if (await _userService.CheckEmailExistsAsync(registerRequest.EmailAddress))
            throw new UnauthorizedAccessException("Email đã được sử dụng");

        ValidateRegisterModel(registerRequest);

        var userForCreationDto = new UserForCreationDto
        {
            UserName = registerRequest.UserName,
            EmailAddress = registerRequest.EmailAddress,
            PhoneNumber = registerRequest.PhoneNumber,
            Password = registerRequest.Password,
            SurName = registerRequest.SurName,
            LastName = registerRequest.LastName,
            Status = "Active", 
        };
        UserDto createdUser = null;

        try
        {
            
            createdUser = await _userService.CreateAsync(userForCreationDto);
            await AssignRoleToUser(createdUser.UserId.ToString(), "customer");
            return createdUser.UserId.ToString();
        }
        catch (Exception ex)
        {
            // Rollback nếu có lỗi
            if (createdUser != null)
                await _userService.DeleteAsync(createdUser.UserId);
        
            throw new ApplicationException("Đăng ký thất bại", ex);
        }
    }
    
    public async Task AssignRoleToUser(string userId, string roleName)
    {
        var userGuid = Guid.Parse(userId);
        var user = await _unitOfWork.Users.GetByIdAsync(userGuid);

        if (user == null || user.IsDeleted)
            throw new KeyNotFoundException($"User với ID {userId} không tồn tại");

        RoleDto roleDto;
        try
        {
            roleDto = await _roleService.GetByNameAsync(roleName);
        }
        catch (KeyNotFoundException)
        {
            roleDto = await _roleService.CreateAsync(new RoleForCreationDto { RoleName = roleName });
        }

        // Chỉ cập nhật RoleId, không ảnh hưởng đến các trường khác
        user.RoleId = roleDto.RoleId;
        user.LastUpdatedTime = DateTimeOffset.UtcNow;
        user.LastUpdatedBy = "System";

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private void ValidateRegisterModel(RegisterRequest registerRequest)
    {
        if (!IsValidEmail(registerRequest.EmailAddress))
        {
            throw new Exception("Invalid Email format");
        }

        if (!IsValidUsername(registerRequest.UserName))
        {
            throw new Exception("Invalid Username format");
        }
        

        if (!IsValidPhoneNumber(registerRequest.PhoneNumber)) {
            throw new Exception("Invalid PhoneNumber format");
        }

        if (string.IsNullOrWhiteSpace(registerRequest.Password) || !IsValidPassword(registerRequest.Password)) {
                throw new Exception("Invalid Password format");
        }
    }


    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
   
    private bool IsValidUsername(string username)
    {
        return !System.Text.RegularExpressions.Regex.IsMatch(username, "[^a-zA-Z0-9]");
    }
    
    private bool IsValidPhoneNumber(string phoneNumber)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, "^0[0-9]{9,10}$");
    }
    private bool IsValidPassword(string password)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}[\]|\\:;'\<>,.?/~`])[A-Za-z\d!@#$%^&*()_+={}[\]|\\:;'\<>,.?/~`]{8,}$");
    }
    
    private  bool IsValidNames(string validRegex, params string?[] credentials)
    {
        foreach (var credential in credentials)
        {
            if (credential is null ||
                credential.TrimStart().Length != credential.Length ||
                credential.TrimEnd().Length != credential.Length ||
                !Regex.IsMatch(credential, validRegex)
               )
                return false;
        }
        return true;
    }
}