using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.Blog;
using Services.Response;

namespace Services.Interface;

public interface IBlogService
{
    Task<BlogWithDetailDto> GetByIdAsync(Guid id);
    Task<PagedResponse<BlogDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<BlogDto> CreateAsync(BlogForCreationDto? addressForCreationDto, Guid userId);
    Task<BlogDto> UpdateAsync(Guid addressId, BlogForUpdateDto addressForUpdateDto, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}