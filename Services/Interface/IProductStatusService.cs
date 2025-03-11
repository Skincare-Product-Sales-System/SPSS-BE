using BusinessObjects.Dto.ProductStatus;
using BusinessObjects.Models;
using Services.Response;

namespace Services.Interface
{
    public interface IProductStatusService
    {
        Task<ProductStatusDto> GetByIdAsync(Guid id);

        Task<PagedResponse<ProductStatusDto>> GetPagedAsync(int pageNumber, int pageSize);

        Task<ProductStatusDto> CreateAsync(ProductStatusForCreationDto productStatusDto);

        Task<ProductStatusDto> UpdateAsync(Guid id, ProductStatusForUpdateDto productStatusDto);

        Task DeleteAsync(Guid id);
    }
}
