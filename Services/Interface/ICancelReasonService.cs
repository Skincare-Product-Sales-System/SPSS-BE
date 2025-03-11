using BusinessObjects.Dto.CancelReason;
using Services.Response;

namespace Services.Interface
{
    public interface ICancelReasonService
    {
        Task<CancelReasonDto> GetByIdAsync(Guid id);

        Task<PagedResponse<CancelReasonDto>> GetPagedAsync(int pageNumber, int pageSize);

        Task<CancelReasonDto> CreateAsync(CancelReasonForCreationDto cancelReasonDto, string userId);

        Task<CancelReasonDto> UpdateAsync(Guid id, CancelReasonForUpdateDto cancelReasonDto, string userId);

        Task DeleteAsync(Guid id, string userId);
    }
}
