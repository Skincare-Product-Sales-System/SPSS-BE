using BusinessObjects.Dto.PromotionTarget;
using Services.Response;

namespace Services.Interface;

public interface IPromotionTargetService
{
    Task<PromotionTargetDto> GetByIdAsync(Guid id);
    Task<PagedResponse<PromotionTargetDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<PromotionTargetDto> CreateAsync(PromotionTargetForCreationDto? promotionTargetForCreationDto);
    Task<PromotionTargetDto> UpdateAsync(Guid promotionTargetId, PromotionTargetForUpdateDto promotionTargetForUpdateDto);
    Task DeleteAsync(Guid id);
}