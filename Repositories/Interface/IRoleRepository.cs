using BusinessObjects.Models;

namespace Repositories.Interface;

public interface IRoleRepository : IRepositoryBase<Role, Guid>
{
    Task<Role?> GetRoleByNameAsync(string roleName);
}