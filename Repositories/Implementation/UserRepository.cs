using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class UserRepository : RepositoryBase<User, int>, IUserRepository
{
    public UserRepository(SPSSContext context) : base(context)
    {
    }
}