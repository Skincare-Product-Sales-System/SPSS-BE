using AutoMapper;
using BusinessObjects.Dto.CartItem;
using BusinessObjects.Dto.ProductCategory;
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

        public async Task<ReviewDto> GetByIdAsync(Guid id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {id} not found.");

            return _mapper.Map<ReviewDto>(review);
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

        public async Task<PagedResponse<ReviewForProductQueryDto>> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize)
        {
            // Tính toán số bản ghi cần bỏ qua
            var skip = (pageNumber - 1) * pageSize;

            // Truy vấn tổng số đánh giá của sản phẩm
            var totalCount = await _unitOfWork.Reviews.Entities
                .Where(r => r.ProductItem.ProductId == productId && !r.IsDeleted)
                .CountAsync();

            // Truy vấn danh sách đánh giá theo ProductId với phân trang
            var reviews = await _unitOfWork.Reviews.Entities
                .Include(ri => ri.ReviewImages)
                .Include(u => u.User)
                .Include(pi => pi.ProductItem)
                    .ThenInclude(p => p.ProductConfigurations)
                        .ThenInclude(c => c.VariationOption)
                .Include(r => r.Reply)
                    .ThenInclude(u => u.User)
                .Where(r => r.ProductItem.ProductId == productId && !r.IsDeleted)
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

        public async Task<ReviewDto> CreateAsync(ReviewForCreationDto reviewDto)
        {
            if (reviewDto == null)
                throw new ArgumentNullException(nameof(reviewDto), "Review data cannot be null.");

            var review = _mapper.Map<Review>(reviewDto);
            review.Id = Guid.NewGuid(); // Gán ID mới
            review.CreatedTime = DateTimeOffset.UtcNow;

            _unitOfWork.Reviews.Add(review);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<ReviewDto> UpdateAsync(ReviewForUpdateDto reviewDto)
        {
            if(reviewDto == null)
                throw new ArgumentNullException(nameof(reviewDto), "Review data cannot be null.");
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewDto.Id);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {reviewDto.Id} not found.");
            _mapper.Map(reviewDto, review);
            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ReviewDto>(review);
        }

        public async Task DeleteAsync(Guid id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {id} not found.");

            _unitOfWork.Reviews.Delete(review);
            await _unitOfWork.SaveChangesAsync();
        }

    }
}
