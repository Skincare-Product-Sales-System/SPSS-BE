using BusinessObjects.Dto.Blog;
using BusinessObjects.Dto.Brand;
using Services.Response;

namespace Services.Interface;

public interface IBrandService
{
    Task<BrandDto> GetByIdAsync(Guid id);
    Task<PagedResponse<BrandDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<BrandDto> CreateAsync(BrandForCreationDto? brandForCreationDto, Guid userId);
    Task<BrandDto> UpdateAsync(Guid addressId, BrandForUpdateDto brandForUpdateDto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}