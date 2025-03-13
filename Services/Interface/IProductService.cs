using BusinessObjects.Dto.Product;
using Services.Response;

namespace Services.Interface;

public interface IProductService
{
    Task<ProductWithDetailsDto> GetByIdAsync(Guid id);
    Task<PagedResponse<ProductDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<bool> CreateAsync(ProductForCreationDto productDto, string userId);
    Task<ProductDto> UpdateAsync(ProductForUpdateDto productDto, string userId); 
    Task DeleteAsync(Guid id, string userId);

    Task<PagedResponse<ProductDto>> GetByCategoryIdPagedAsync(Guid categoryId, int pageNumber, int pageSize);
}