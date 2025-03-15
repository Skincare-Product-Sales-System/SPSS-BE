using BusinessObjects.Dto.Role;
using BusinessObjects.Dto.User;
using Services.Response;

namespace Services.Interface;

public interface IRoleService
{
    Task<RoleDto> GetByIdAsync(Guid id);
    Task<PagedResponse<RoleDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<RoleDto> CreateAsync(RoleForCreationDto? roleForCreationDto);
    Task<RoleDto> UpdateAsync(Guid roleId, RoleForUpdateDto roleForUpdateDto);
    Task DeleteAsync(Guid id);
    Task<RoleDto> GetByNameAsync(string roleName);
}