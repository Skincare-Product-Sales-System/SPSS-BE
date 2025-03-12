using BusinessObjects.Dto.User;
using BusinessObjects.Models;

namespace Repositories.Interface;

public interface IUserRepository : IRepositoryBase<User, Guid>
{
    Task<User> GetByEmailAsync(string email);
    Task<User> GetByUserNameAsync(string userName);
}