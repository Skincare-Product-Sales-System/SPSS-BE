using AutoMapper;
using BusinessObjects.Dto.CartItem;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.Reply;
using BusinessObjects.Dto.Review;
using BusinessObjects.Models;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PagedResponse<ReviewDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            // Tính toán số bản ghi cần bỏ qua
            var skip = (pageNumber - 1) * pageSize;

            // Truy vấn tổng số bản ghi
            var totalCount = await _unitOfWork.Reviews.Entities
                .Where(r => !r.IsDeleted)
                .CountAsync();

            // Truy vấn dữ liệu với phân trang
            var cartItems = await _unitOfWork.Reviews.Entities
                .Include(ri => ri.ReviewImages)
                .Include(u => u.User)
                .Include(pi => pi.ProductItem)
                    .ThenInclude(p => p.ProductConfigurations)
                        .ThenInclude(c => c.VariationOption)
                .Include(r => r.Reply)
                    .ThenInclude(u => u.User)
                .Include(r => r.ProductItem).
                    ThenInclude(p => p.Product)
                        .ThenInclude(c => c.ProductImages)
                .Where(r =>!r.IsDeleted)
                .OrderByDescending(r => r.LastUpdatedTime)
                .Skip(skip)
                .Take(pageSize) 
                .ToListAsync();

            var mappedItems = _mapper.Map<IEnumerable<ReviewDto>>(cartItems);

            return new PagedResponse<ReviewDto>
            {
                Items = mappedItems,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResponse<ReviewDto>> GetPagedByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            // Tính toán số bản ghi cần bỏ qua
            var skip = (pageNumber - 1) * pageSize;

            // Truy vấn tổng số bản ghi của người dùng
            var totalCount = await _unitOfWork.Reviews.Entities
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .CountAsync();

            // Truy vấn dữ liệu với phân trang cho userId
            var userReviews = await _unitOfWork.Reviews.Entities
                .Include(ri => ri.ReviewImages)
                .Include(u => u.User)
                .Include(pi => pi.ProductItem)
                    .ThenInclude(p => p.ProductConfigurations)
                        .ThenInclude(c => c.VariationOption)
                .Include(r => r.Reply)
                    .ThenInclude(u => u.User)
                .Include(r => r.ProductItem)
                    .ThenInclude(p => p.Product)
                        .ThenInclude(c => c.ProductImages)
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .OrderByDescending(r => r.LastUpdatedTime)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Manually map data to ReviewDto and set IsEditable
            var reviewDtos = userReviews.Select(review => new ReviewDto
            {
                Id = review.Id,
                UserName = review.User.UserName,
                AvatarUrl = review.User.AvatarUrl,
                ProductImage = review.ProductItem.Product.ProductImages.FirstOrDefault()?.ImageUrl,
                ProductId = review.ProductItem.ProductId,
                ProductName = review.ProductItem.Product.Name,
                ReviewImages = review.ReviewImages.Select(ri => ri.ImageUrl).ToList(),
                VariationOptionValues = review.ProductItem.ProductConfigurations.Select(pc => pc.VariationOption.Value).ToList(),
                RatingValue = review.RatingValue,
                Comment = review.Comment,
                LastUpdatedTime = review.LastUpdatedTime,
                Reply = review.Reply != null ? new ReplyDto
                {
                    Id = review.Reply.Id,
                    AvatarUrl = review.Reply.User.AvatarUrl,
                    UserName = review.Reply.User.UserName,
                    ReplyContent = review.Reply.ReplyContent,
                    LastUpdatedTime = review.Reply.LastUpdatedTime
                } : null,
                IsEditble = review.CreatedTime.HasValue && review.LastUpdatedTime.HasValue &&
            review.CreatedTime.Value.ToUniversalTime().AddTicks(-(review.CreatedTime.Value.Ticks % TimeSpan.TicksPerSecond)) ==
            review.LastUpdatedTime.Value.ToUniversalTime().AddTicks(-(review.LastUpdatedTime.Value.Ticks % TimeSpan.TicksPerSecond)),
            }).ToList();

            // Trả về kết quả phân trang
            return new PagedResponse<ReviewDto>
            {
                Items = reviewDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> GetTotalReviewsByUserIdAsync(Guid userId)
        {
            // Đếm tổng số review của người dùng với điều kiện không bị xóa
            var totalReviews = await _unitOfWork.Reviews.Entities
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .CountAsync();

            return totalReviews;
        }

        public async Task<PagedResponse<ReviewForProductQueryDto>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize, int? ratingFilter = null)
        {
            // Tính toán số bản ghi cần bỏ qua
            var skip = (pageNumber - 1) * pageSize;

            // Truy vấn danh sách đánh giá của sản phẩm (áp dụng bộ lọc rating nếu có)
            var query = _unitOfWork.Reviews.Entities
                .Include(ri => ri.ReviewImages)
                .Include(u => u.User)
                .Include(pi => pi.ProductItem)
                    .ThenInclude(p => p.ProductConfigurations)
                        .ThenInclude(c => c.VariationOption)
                .Include(r => r.Reply)
                    .ThenInclude(u => u.User)
                .Where(r => r.ProductItem.ProductId == productId && !r.IsDeleted);

            // Thêm điều kiện lọc rating nếu được chỉ định
            if (ratingFilter.HasValue)
            {
                query = query.Where(r => r.RatingValue == ratingFilter.Value);
            }

            // Truy vấn tổng số đánh giá sau khi áp dụng bộ lọc
            var totalCount = await query.CountAsync();

            // Truy vấn danh sách đánh giá theo ProductId với phân trang
            var reviews = await query
                .OrderByDescending(r => r.LastUpdatedTime)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Ánh xạ dữ liệu sang DTO
            var reviewDtos = _mapper.Map<IEnumerable<ReviewForProductQueryDto>>(reviews);

            // Trả về kết quả phân trang
            return new PagedResponse<ReviewForProductQueryDto>
            {
                Items = reviewDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ReviewForCreationDto> CreateAsync(Guid userId, ReviewForCreationDto reviewDto)
        {
            if (reviewDto == null)
                throw new ArgumentNullException(nameof(reviewDto), "Review data cannot be null.");

            // Map the review data
            var review = _mapper.Map<Review>(reviewDto);
            review.Id = Guid.NewGuid();
            review.UserId = userId;
            review.CreatedBy = userId.ToString();
            review.LastUpdatedBy = userId.ToString();

            // Add the review to the database
            _unitOfWork.Reviews.Add(review);

            // Add ReviewImages if they exist
            if (reviewDto.ReviewImages != null && reviewDto.ReviewImages.Any())
            {
                review.ReviewImages = reviewDto.ReviewImages.Select(imageUrl => new ReviewImage
                {
                    Id = Guid.NewGuid(),
                    ReviewId = review.Id,
                    ImageUrl = imageUrl
                }).ToList();

                // Add ReviewImages to the database
                foreach (var reviewImage in review.ReviewImages)
                {
                    _unitOfWork.ReviewImages.Add(reviewImage);
                }
            }

            // Save changes to persist the new review and images
            await _unitOfWork.SaveChangesAsync();

            // Query the ProductItem to get the associated ProductId
            var productItem = await _unitOfWork.ProductItems.Entities
                .FirstOrDefaultAsync(pi => pi.Id == reviewDto.ProductItemId);

            if (productItem == null)
                throw new KeyNotFoundException($"ProductItem with ID {reviewDto.ProductItemId} not found.");

            // Query all reviews for the product, including the newly added review
            var productReviews = await _unitOfWork.Reviews.Entities
                .Where(r => r.ProductItem.ProductId == productItem.ProductId && !r.IsDeleted)
                .ToListAsync();

            // Calculate the average rating from all reviews for the product
            var averageRating = productReviews.Any()
                ? productReviews.Average(r => r.RatingValue)
                : 0; // Default to 0 if there are no reviews

            // Update the product's rating
            var product = await _unitOfWork.Products.Entities
                .FirstOrDefaultAsync(p => p.Id == productItem.ProductId);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productItem.ProductId} not found.");

            product.Rating = averageRating;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            // Return the mapped DTO
            return _mapper.Map<ReviewForCreationDto>(review);
        }

        public async Task<ReviewDto> UpdateAsync(Guid userId, ReviewForUpdateDto reviewDto, Guid id)
        {
            if (reviewDto == null)
                throw new ArgumentNullException(nameof(reviewDto), "Review data cannot be null.");

            // Fetch review with related ReviewImages
            var review = await _unitOfWork.Reviews
                .GetQueryable()
                .Include(r => r.ReviewImages)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                throw new KeyNotFoundException($"Review with ID {id} not found.");

            // Map data from DTO to Review entity
            _mapper.Map(reviewDto, review);

            review.LastUpdatedTime = DateTimeOffset.UtcNow;
            review.LastUpdatedBy = userId.ToString();

            // If there are no images in the DTO, initialize to an empty list
            var updatedImages = reviewDto.ReviewImages ?? new List<string>();

            // Determine images that need to be removed (exist in DB but not in the updated list)
            var imagesToRemove = review.ReviewImages
                .Where(existing => !updatedImages.Contains(existing.ImageUrl, StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var image in imagesToRemove)
            {
                // Remove from ReviewImages collection
                review.ReviewImages.Remove(image);
                // Delete from database
                _unitOfWork.ReviewImages.Delete(image);
            }

            // Determine images that are new (present in updated DTO but not in the existing DB collection)
            var newImages = updatedImages
                .Where(imageUrl => !review.ReviewImages.Any(existing => string.Equals(existing.ImageUrl, imageUrl, StringComparison.OrdinalIgnoreCase)))
                .Select(imageUrl => new ReviewImage
                {
                    Id = Guid.NewGuid(),
                    ReviewId = review.Id,
                    ImageUrl = imageUrl
                })
                .ToList();

            // Add new images to the review and database
            foreach (var newImage in newImages)
            {
                review.ReviewImages.Add(newImage);
                _unitOfWork.ReviewImages.Add(newImage);
            }

            // Update the review entity in the unit of work
            _unitOfWork.Reviews.Update(review);

            // Commit changes to the database
            await _unitOfWork.SaveChangesAsync();

            // Return the updated ReviewDto
            return _mapper.Map<ReviewDto>(review);
        }

        public async Task DeleteAsync(Guid userId, Guid id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {id} not found.");
            review.IsDeleted = true;
            review.DeletedBy = userId.ToString();
            review.DeletedTime = DateTimeOffset.UtcNow;

            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();
        }

    }
}
