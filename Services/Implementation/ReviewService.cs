using AutoMapper;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.Review;
using BusinessObjects.Models;
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
            var (reviews, totalCount) = await _unitOfWork.Reviews.GetPagedAsync(
                pageNumber,
                pageSize,
                cr => cr.IsDeleted == false
            );
            var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
            return new PagedResponse<ReviewDto>
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
