using BusinessObjects.Dto.PromotionType;
using Services.Response;

namespace Services.Interface
{
    public interface IPromotionTypeService
    {
        Task<PromotionTypeDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PromotionTypeDto>> GetAllAsync();
        Task<PromotionTypeDto> CreateAsync(PromotionTypeForCreationDto promotionTypeDto, string userId);
        Task<PromotionTypeDto> UpdateAsync(Guid id, PromotionTypeForUpdateDto promotionTypeDto, string userId);
        Task DeleteAsync(Guid id, string userId);
    }
}
