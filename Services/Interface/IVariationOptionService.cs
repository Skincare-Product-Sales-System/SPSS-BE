using BusinessObjects.Dto.VariationOption;
using Services.Response;

namespace Services.Interface
{
    public interface IVariationOptionService
    {
        Task<VariationOptionDto> GetByIdAsync(Guid id);
        Task<PagedResponse<VariationOptionDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<VariationOptionDto> CreateAsync(VariationOptionForCreationDto variationOptionDto, string userId);
        Task<VariationOptionDto> UpdateAsync(Guid id, VariationOptionForUpdateDto variationOptionDto, string userId);
        Task DeleteAsync(Guid id, string userId);
    }
}
