using BusinessObjects.Dto.User;
using BusinessObjects.Models;

namespace Repositories.Interface;

public interface IUserRepository : IRepositoryBase<User, int>
{
    Task<UserDto> GetByEmailAsync(string email);
    Task<UserDto> GetByUserNameAsync(string userName);
}