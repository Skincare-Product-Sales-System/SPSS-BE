using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.SkinType;
using Services.Response;

namespace Services.Interface;

public interface ISkinTypeService
{
    Task<SkinTypeWithDetailDto> GetByIdAsync(Guid id);
    Task<PagedResponse<SkinTypeDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<bool> CreateAsync(SkinTypeForCreationDto? skinTypeForCreationDto, Guid userId);
    Task<SkinTypeWithDetailDto> UpdateAsync(Guid addressId, SkinTypeForUpdateDto skinTypeForUpdateDto);
    Task DeleteAsync(Guid id);
}