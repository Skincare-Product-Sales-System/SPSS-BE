using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class AccountRepository : RepositoryBase<User, Guid>, IAccountRepository
    {
        public AccountRepository(SPSSContext context) : base(context)
        {
        }
    }
}
