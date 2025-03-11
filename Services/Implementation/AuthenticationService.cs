using BusinessObjects.Dto.Authentication;
using Services.Interface;

namespace Services.Implementation;

public class AuthenticationService : IAuthenticationService
{
    public Task<LoginResponse> Login(LoginDto request)
    {
          try
            {
                var user = await _dbPolicyWrap.ExecuteAsync(async () => await _userManager.FindByEmailAsync(model.Email));

                if (user == null)
                {
                    throw new ArgumentNullException("Invalid email or password");
                }

                if (user.LockoutEnabled == false)
                {
                    throw new ArgumentException("User is banned!");
                }

                var result = await _dbPolicyWrap.ExecuteAsync(async () => await _userManager.CheckPasswordAsync(user, model.Password));

                if (!result)
                {
                    user.AccessFailedCount++;
                    if (user.AccessFailedCount == 5)
                    {
                        user.LockoutEnabled = false;
                    }
                    await _dbPolicyWrap.ExecuteAsync(async () => await _userManager.UpdateAsync(user));

                    throw new ArgumentException("Invalid password!");
                }

                var roles = await _dbPolicyWrap.ExecuteAsync(async () => await _userManager.GetRolesAsync(user));
                var userRole = roles.FirstOrDefault();
                var token = GenerateJWT.GenerateToken(user, userRole);
                var refreshToken = Guid.NewGuid().ToString();
                var tokenExpiration = DateTime.Now.AddDays(30);

                var userDetail = await _userDetailService.GetUserById(user.Id);
                userDetail.RefreshToken = refreshToken;
                userDetail.RefreshTokenExpiration = tokenExpiration;
                await _userDetailService.UpdateUserDetail(userDetail);

                return new LoginDto
                {
                    Id = user.Id,
                    Token = token,
                    Role = userRole,
                    RefreshToken = refreshToken,
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error logging in: {ex.Message}");
            }
    }

    public Task RegisterSystemAccount(RegisterDto model)
    {
        throw new NotImplementedException();
    }
}