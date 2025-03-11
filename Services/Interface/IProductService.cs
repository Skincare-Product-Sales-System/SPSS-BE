using BusinessObjects.Dto.Product;
using Services.Response;

namespace Services.Interface;

public interface IProductService
{
    Task<ProductWithDetailsDto> GetByIdAsync(Guid id);
    Task<PagedResponse<ProductDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<ProductWithDetailsDto> CreateAsync(ProductForCreationDto productDto);
    Task<ProductDto> UpdateAsync(ProductForUpdateDto productDto); 
    Task DeleteAsync(Guid id);
}