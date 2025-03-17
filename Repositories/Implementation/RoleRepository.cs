using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Implementation;

public class RoleRepository : RepositoryBase<Role, Guid>, IRoleRepository
{
    public RoleRepository(SPSSContext context) : base(context)
    {
        
    }
    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == roleName);
    }
}