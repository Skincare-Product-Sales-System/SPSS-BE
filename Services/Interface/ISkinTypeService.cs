using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.SkinType;
using Services.Response;

namespace Services.Interface;

public interface ISkinTypeService
{
    Task<SkinTypeDto> GetByIdAsync(Guid id);
    Task<PagedResponse<SkinTypeDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<SkinTypeDto> CreateAsync(SkinTypeForCreationDto? skinTypeForCreationDto);
    Task<SkinTypeDto> UpdateAsync(Guid addressId, SkinTypeForUpdateDto skinTypeForUpdateDto);
    Task DeleteAsync(Guid id);
}