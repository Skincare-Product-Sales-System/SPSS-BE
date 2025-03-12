using BusinessObjects.Dto.User;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Implementation;

public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
{
    public UserRepository(SPSSContext context) : base(context)
    {
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        var user = await _context.Users
            .Where(u => u.EmailAddress == email && !u.IsDeleted)
            .FirstOrDefaultAsync();
        return user;
    }

    public async Task<User> GetByUserNameAsync(string userName)
    {
        var user = await _context.Users
            .Where(u => u.UserName == userName && !u.IsDeleted)
            .FirstOrDefaultAsync();
        return user;
    }
}