using BusinessObjects.Dto.Authentication;

namespace Services.Interface;

public interface IAuthenticationService
{
    Task<LoginResponse> Login(LoginDto request);
    Task RegisterSystemAccount(RegisterDto model);
}