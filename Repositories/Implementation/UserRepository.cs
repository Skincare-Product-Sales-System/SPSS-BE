using BusinessObjects.Dto.User;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Implementation;

public class UserRepository : RepositoryBase<User, int>, IUserRepository
{
    public UserRepository(SPSSContext context) : base(context)
    {
    }

    public async Task<UserDto> GetByEmailAsync(string email)
    {
        var user = await _context.Users
            .Where(u => u.EmailAddress == email && !u.IsDeleted)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                EmailAddress = u.EmailAddress,
                SurName = u.SurName,
                LastName = u.LastName,
            })
            .FirstOrDefaultAsync();
        return user;
    }

    public async Task<UserDto> GetByUserNameAsync(string userName)
    {
        var user = await _context.Users
            .Where(u => u.UserName == userName && !u.IsDeleted)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                EmailAddress = u.EmailAddress,
                SurName = u.SurName,
                LastName = u.LastName,
            })
            .FirstOrDefaultAsync();
        return user;
    }
}