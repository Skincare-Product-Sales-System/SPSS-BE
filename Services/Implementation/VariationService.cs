using BusinessObjects.Dto.Variation;
using Services.Interface;
using Services.Response;
using AutoMapper;
using BusinessObjects.Models;
using Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.VariationOption;

namespace Services.Implementation
{
    public class VariationService : IVariationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VariationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<VariationDto> GetByIdAsync(Guid id)
        {
            var variation = await _unitOfWork.Variations.GetByIdAsync(id);
            if (variation == null)
                throw new KeyNotFoundException($"Variation with ID {id} not found.");

            return _mapper.Map<VariationDto>(variation);
        }

        public async Task<PagedResponse<VariationDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            // Truy vấn danh sách Variations bao gồm ProductCategory
            var variationsQuery = _unitOfWork.Variations.Entities
                .Include(v => v.ProductCategory) // Include ProductCategory liên quan
                .Include(v => v.VariationOptions) // Include VariationOptions
                .Where(v => !v.IsDeleted);

            // Lấy tổng số bản ghi phù hợp
            var totalCount = await variationsQuery.CountAsync();

            // Lấy dữ liệu cho trang hiện tại
            var variations = await variationsQuery
                .OrderBy(v => v.Id) // Sắp xếp để đảm bảo thứ tự cố định
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Ánh xạ thủ công từng Variation sang VariationDto
            var mappedVariations = variations.Select(v => new VariationDto
            {
                Id = v.Id,
                Name = v.Name,
                ProductCategory = v.ProductCategory == null ? null : new CategoryForVariationQuery
                {
                    Id = v.ProductCategory.Id,
                    CategoryName = v.ProductCategory.CategoryName
                },
                VariationOptions = v.VariationOptions.Select(vo => new VariationOptionForVariationQuery
                {
                    Id = vo.Id,
                    Value = vo.Value
                }).ToList()
            });

            // Trả về kết quả dưới dạng PagedResponse
            return new PagedResponse<VariationDto>
            {
                Items = mappedVariations,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<VariationDto> CreateAsync(VariationForCreationDto variationDto, string userId)
        {
            if (variationDto == null)
                throw new ArgumentNullException(nameof(variationDto));

            var variationEntity = _mapper.Map<Variation>(variationDto);
            variationEntity.CreatedBy = userId;
            variationEntity.CreatedTime = DateTime.UtcNow;

             _unitOfWork.Variations.Add(variationEntity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<VariationDto>(variationEntity);
        }

        public async Task<VariationDto> UpdateAsync(Guid id, VariationForUpdateDto variationDto, string userId)
        {
            if (variationDto == null)
                throw new ArgumentNullException(nameof(variationDto));

            var existingVariation = await _unitOfWork.Variations.GetByIdAsync(id);
            if (existingVariation == null)
                throw new KeyNotFoundException($"Variation with ID {id} not found.");

            _mapper.Map(variationDto, existingVariation);
            existingVariation.LastUpdatedBy = userId;
            existingVariation.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.Variations.Update(existingVariation);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<VariationDto>(existingVariation);
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            var variation = await _unitOfWork.Variations.GetByIdAsync(id);
            if (variation == null)
                throw new KeyNotFoundException($"Variation with ID {id} not found.");

            variation.IsDeleted = true;
            variation.LastUpdatedBy = userId;
            variation.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.Variations.Update(variation);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}