using BusinessObjects.Dto.VariationOption;
using Services.Interface;
using Services.Response;
using AutoMapper;
using BusinessObjects.Models;
using Repositories.Interface;

namespace Services.Implementation
{
    public class VariationOptionService : IVariationOptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VariationOptionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<VariationOptionDto> GetByIdAsync(Guid id)
        {
            var variationOption = await _unitOfWork.VariationOptions.GetByIdAsync(id);
            if (variationOption == null)
                throw new KeyNotFoundException($"Variation option with ID {id} not found.");

            return _mapper.Map<VariationOptionDto>(variationOption);
        }

        public async Task<PagedResponse<VariationOptionDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var variationOptions = await _unitOfWork.VariationOptions.GetPagedAsync(pageNumber, pageSize, v => v.IsDeleted == false);
            var mappedOptions = _mapper.Map<IEnumerable<VariationOptionDto>>(variationOptions.Items);

            return new PagedResponse<VariationOptionDto>
            {
                Items = mappedOptions,
                TotalCount = variationOptions.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<VariationOptionDto> CreateAsync(VariationOptionForCreationDto variationOptionDto, string userId)
        {
            if (variationOptionDto == null)
                throw new ArgumentNullException(nameof(variationOptionDto));

            var variationOptionEntity = _mapper.Map<VariationOption>(variationOptionDto);
            variationOptionEntity.CreatedBy = userId;
            variationOptionEntity.CreatedTime = DateTime.UtcNow;

            _unitOfWork.VariationOptions.Add(variationOptionEntity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<VariationOptionDto>(variationOptionEntity);
        }

        public async Task<VariationOptionDto> UpdateAsync(Guid id, VariationOptionForUpdateDto variationOptionDto, string userId)
        {
            if (variationOptionDto == null)
                throw new ArgumentNullException(nameof(variationOptionDto));

            var existingVariationOption = await _unitOfWork.VariationOptions.GetByIdAsync(id);
            if (existingVariationOption == null)
                throw new KeyNotFoundException($"Variation option with ID {id} not found.");

            _mapper.Map(variationOptionDto, existingVariationOption);
            existingVariationOption.LastUpdatedBy = userId;
            existingVariationOption.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.VariationOptions.Update(existingVariationOption);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<VariationOptionDto>(existingVariationOption);
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            var variationOption = await _unitOfWork.VariationOptions.GetByIdAsync(id);
            if (variationOption == null)
                throw new KeyNotFoundException($"Variation option with ID {id} not found.");

            variationOption.IsDeleted = true;
            variationOption.LastUpdatedBy = userId;
            variationOption.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.VariationOptions.Update(variationOption);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
