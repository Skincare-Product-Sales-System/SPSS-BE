using AutoMapper;
using BusinessObjects.Dto.Role;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RoleDto> GetByIdAsync(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null || role.IsDeleted)
            throw new KeyNotFoundException($"Role with ID {id} not found.");
        return _mapper.Map<RoleDto>(role);
    }

    public async Task<PagedResponse<RoleDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (roles, totalCount) = await _unitOfWork.Roles.GetPagedAsync(pageNumber, pageSize, r => r.IsDeleted == false);
        var roleDtos = _mapper.Map<IEnumerable<RoleDto>>(roles);
        return new PagedResponse<RoleDto>
        {
            Items = roleDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<RoleDto> CreateAsync(RoleForCreationDto? roleForCreationDto)
    {
        if (roleForCreationDto is null)
            throw new ArgumentNullException(nameof(roleForCreationDto), "Role data cannot be null.");

        var role = _mapper.Map<Role>(roleForCreationDto);
        role.CreatedTime = DateTimeOffset.UtcNow;
        role.CreatedBy = "System"; // Replace with actual user if available
        role.IsDeleted = false;

        _unitOfWork.Roles.Add(role);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> UpdateAsync(Guid roleId, RoleForUpdateDto roleForUpdateDto)
    {
        if (roleForUpdateDto is null)
            throw new ArgumentNullException(nameof(roleForUpdateDto), "Role data cannot be null.");

        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Role with ID {roleId} not found.");

        role.LastUpdatedTime = DateTimeOffset.UtcNow;
        role.LastUpdatedBy = "System"; // Replace with actual user if available

        _mapper.Map(roleForUpdateDto, role);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<RoleDto>(role);
    }

   
    public async Task DeleteAsync(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
            throw new KeyNotFoundException($"Role with ID {id} not found.");

        role.IsDeleted = true;
        role.DeletedTime = DateTimeOffset.UtcNow;
        role.DeletedBy = "System"; // Replace with actual user if available

        _unitOfWork.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync();
    }
}
