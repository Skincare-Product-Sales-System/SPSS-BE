using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductCategory;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IProductCategoryService
    {
        Task<ProductCategoryDto> GetByIdAsync(Guid id);
        Task<PagedResponse<ProductCategoryDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<ProductCategoryDto> CreateAsync(ProductCategoryForCreationDto productDto);
        Task<ProductCategoryDto> UpdateAsync(ProductCategoryForUpdateDto productDto);
        Task DeleteAsync(Guid id);
    }
}
