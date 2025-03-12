using BusinessObjects.Dto.Variation;
using Services.Interface;
using Services.Response;
using AutoMapper;
using BusinessObjects.Models;
using Repositories.Interface;

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
            var variations = await _unitOfWork.Variations.GetPagedAsync(pageNumber, pageSize, v => v.IsDeleted == false);
            var mappedVariations = _mapper.Map<IEnumerable<VariationDto>>(variations.Items);

            return new PagedResponse<VariationDto>
            {
                Items = mappedVariations,
                TotalCount = variations.TotalCount,
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
