using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.Blog;
using Services.Response;

namespace Services.Interface;

public interface IBlogService
{
    Task<BlogDto> GetByIdAsync(Guid id);
    Task<PagedResponse<BlogDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<BlogDto> CreateAsync(BlogForCreationDto? addressForCreationDto);
    Task<BlogDto> UpdateAsync(Guid addressId, BlogForUpdateDto addressForUpdateDto);
    Task DeleteAsync(Guid id);
}