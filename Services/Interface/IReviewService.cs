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
        Task<PagedResponse<ReviewDto>> GetPagedByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<PagedResponse<ReviewForProductQueryDto>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize, int? ratingFilter = null);
        Task<PagedResponse<ReviewDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<ReviewForCreationDto> CreateAsync(Guid userId, ReviewForCreationDto reviewDto);
        Task<ReviewDto> UpdateAsync(Guid userId, ReviewForUpdateDto reviewDto, Guid id);
        Task DeleteAsync(Guid userId, Guid id);
    }
}
