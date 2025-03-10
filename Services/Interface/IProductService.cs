using BusinessObjects.Dto.Product;
using Services.Response;

namespace Services.Interface;

public interface IProductService
{
    Task<ProductDto> GetByIdAsync(Guid id);
    Task<PagedResponse<ProductDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<ProductDto> CreateAsync(ProductForCreationDto productDto);
    Task<ProductDto> UpdateAsync(ProductForUpdateDto productDto); 
    Task DeleteAsync(Guid id);
}