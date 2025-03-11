using AutoMapper;
using BusinessObjects.Dto.PromotionType;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation
{
    public class PromotionTypeService : IPromotionTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PromotionTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PromotionTypeDto> GetByIdAsync(Guid id)
        {
            var promotionType = await _unitOfWork.PromotionTypes.GetByIdAsync(id);
            if (promotionType == null)
                throw new KeyNotFoundException($"Promotion Type with ID {id} not found or has been deleted.");

            return _mapper.Map<PromotionTypeDto>(promotionType);
        }

        public async Task<IEnumerable<PromotionTypeDto>> GetAllAsync()
        {
            var promotionTypes = await _unitOfWork.PromotionTypes.GetAllAsync();

            if (promotionTypes == null || !promotionTypes.Any())
                throw new KeyNotFoundException("No promotion types found.");

            return _mapper.Map<IEnumerable<PromotionTypeDto>>(promotionTypes);
        }


        public async Task<PromotionTypeDto> CreateAsync(PromotionTypeForCreationDto promotionTypeDto, string userId)
        {
            if (promotionTypeDto == null)
                throw new ArgumentNullException(nameof(promotionTypeDto), "Promotion Type data cannot be null.");

            var promotionType = _mapper.Map<PromotionType>(promotionTypeDto);

            //promotionType.CreatedBy = userId;
            //promotionType.LastUpdatedBy = userId;
            //promotionType.CreatedTime = DateTime.UtcNow;
            //promotionType.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.PromotionTypes.Add(promotionType);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PromotionTypeDto>(promotionType);
        }

        public async Task<PromotionTypeDto> UpdateAsync(Guid id, PromotionTypeForUpdateDto promotionTypeDto, string userId)
        {
            if (promotionTypeDto == null)
                throw new ArgumentNullException(nameof(promotionTypeDto), "Promotion Type data cannot be null.");

            var promotionType = await _unitOfWork.PromotionTypes.GetByIdAsync(id);
            if (promotionType == null)
                throw new KeyNotFoundException($"Promotion Type with ID {id} not found or has been deleted.");

            //promotionType.LastUpdatedBy = userId;
            //promotionType.LastUpdatedTime = DateTime.UtcNow;

            _mapper.Map(promotionTypeDto, promotionType);
            _unitOfWork.PromotionTypes.Update(promotionType);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PromotionTypeDto>(promotionType);
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            var promotionType = await _unitOfWork.PromotionTypes.GetByIdAsync(id);
            if (promotionType == null)
                throw new KeyNotFoundException($"Promotion Type with ID {id} not found or has been deleted.");

            //promotionType.IsDeleted = true;
            //promotionType.DeletedBy = userId;

            _unitOfWork.PromotionTypes.Delete(promotionType); // Soft delete via update
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
