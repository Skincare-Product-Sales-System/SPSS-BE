using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.Review;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IReviewService
    {
        Task<ReviewDto> GetByIdAsync(Guid id);
        Task<PagedResponse<ReviewDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<ReviewDto> CreateAsync(ReviewForCreationDto reviewDto);
        Task<ReviewDto> UpdateAsync(ReviewForUpdateDto reviewDto);
        Task DeleteAsync(Guid id);
    }
}
