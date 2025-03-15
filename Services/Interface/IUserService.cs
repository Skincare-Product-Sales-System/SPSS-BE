using BusinessObjects.Dto.User;
using Services.Response;

namespace Services.Interface;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> GetByEmailAsync(string email);
    Task<UserDto> GetByUserNameAsync(string userName);
    Task<PagedResponse<UserDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<UserDto> CreateAsync(UserForCreationDto? userForCreationDto);
    Task<UserDto> UpdateAsync(Guid userId, UserForUpdateDto userForUpdateDto);
    Task DeleteAsync(Guid id);
    Task<bool> CheckUserNameExistsAsync(string userName);
    Task<bool> CheckEmailExistsAsync(string email);
}