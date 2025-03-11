using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.User;
using Services.Response;

namespace Services.Interface;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(int id);
    Task<PagedResponse<UserDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<UserDto> CreateAsync(UserForCreationDto? userForCreationDto);
    Task<UserDto> UpdateAsync(int userId, UserForUpdateDto userForUpdateDto);
    Task DeleteAsync(int id);
    Task<UserDto> GetByEmailAsync(string email);
    Task<UserDto> GetByUserNameAsync(string userName);
}