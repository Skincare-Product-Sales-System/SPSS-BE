using BusinessObjects.Dto.Variation;
using Services.Response;

namespace Services.Interface
{
    public interface IVariationService
    {
        Task<VariationDto> GetByIdAsync(Guid id);
        Task<PagedResponse<VariationDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<VariationDto> CreateAsync(VariationForCreationDto variationDto, string userId);
        Task<VariationDto> UpdateAsync(Guid id, VariationForUpdateDto variationDto, string userId);
        Task DeleteAsync(Guid id, string userId);
    }
}
