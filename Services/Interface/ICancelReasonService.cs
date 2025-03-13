using BusinessObjects.Dto.CancelReason;
using Services.Response;

namespace Services.Interface
{
    public interface ICancelReasonService
    {
        Task<CancelReasonDto> GetByIdAsync(Guid id);

        Task<PagedResponse<CancelReasonDto>> GetPagedAsync(int pageNumber, int pageSize);

        Task<CancelReasonDto> CreateAsync(CancelReasonForCreationDto cancelReasonDto, Guid userId);

        Task<CancelReasonDto> UpdateAsync(Guid id, CancelReasonForUpdateDto cancelReasonDto, Guid userId);

        Task DeleteAsync(Guid id, Guid userId);
    }
}
