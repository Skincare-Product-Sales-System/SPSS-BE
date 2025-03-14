using BusinessObjects.Dto.Brand;
using BusinessObjects.Dto.Promotion;
using Services.Response;

namespace Services.Interface;

public interface IPromotionService 
{
    Task<PromotionDto> GetByIdAsync(Guid id);
    Task<PagedResponse<PromotionDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<PromotionDto> CreateAsync(PromotionForCreationDto? promotionForCreationDto);
    Task<PromotionDto> UpdateAsync(Guid promotionId, PromotionForUpdateDto promotionForUpdateDto);
    Task DeleteAsync(Guid id);
}