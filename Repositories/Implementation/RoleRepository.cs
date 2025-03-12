using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class RoleRepository : RepositoryBase<Role, Guid>, IRoleRepository
{
    public RoleRepository(SPSSContext context) : base(context)
    {
    }
}