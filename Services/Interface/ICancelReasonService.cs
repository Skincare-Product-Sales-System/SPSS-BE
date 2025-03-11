using BusinessObjects.Dto.CancelReason;
using Services.Response;

namespace Services.Interface
{
    public interface ICancelReasonService
    {
        Task<CancelReasonDto> GetByIdAsync(Guid id);

        Task<PagedResponse<CancelReasonDto>> GetPagedAsync(int pageNumber, int pageSize);

        Task<CancelReasonDto> CreateAsync(CancelReasonForCreationDto cancelReasonDto);

        Task<CancelReasonDto> UpdateAsync(CancelReasonForUpdateDto cancelReasonDto);

        Task DeleteAsync(Guid id);
    }
}