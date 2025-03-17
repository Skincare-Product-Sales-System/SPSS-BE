using BusinessObjects.Dto.Product;
using Services.Response;

namespace Services.Interface;

public interface IProductService
{
    Task<PagedResponse<ProductDto>> GetPagedByBrandAsync(Guid brandId, int pageNumber, int pageSize);
    Task<PagedResponse<ProductDto>> GetPagedBySkinTypeAsync(Guid skinTypeId, int pageNumber, int pageSize);
    Task<PagedResponse<ProductDto>> GetPagedBySkinTypeAndCategoryAsync(Guid skinTypeId, Guid categoryId, int pageNumber, int pageSize);
    Task<PagedResponse<ProductDto>> GetByCategoryIdPagedAsync(Guid categoryId, int pageNumber, int pageSize);
    Task<ProductWithDetailsDto> GetByIdAsync(Guid id);
    Task<PagedResponse<ProductDto>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    Guid? brandId = null,
    Guid? categoryId = null,
    Guid? skinTypeId = null,
    string sortBy = "newest"); // Thêm tham số sortBy với giá trị mặc định "newest"
    Task<bool> CreateAsync(ProductForCreationDto productDto, string userId);
    Task<bool> UpdateAsync(ProductForUpdateDto productDto, Guid userId, Guid productId);
    Task DeleteAsync(Guid id, string userId);
    Task<PagedResponse<ProductDto>> GetBestSellerAsync(int pageNumber, int pageSize);
}