using BusinessObjects.Dto.PaymentMethod;
using Services.Response;

namespace Services.Interface
{
    public interface IPaymentMethodService
    {
        Task<PaymentMethodDto> GetByIdAsync(Guid id);
        Task<PagedResponse<PaymentMethodDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<PaymentMethodDto> CreateAsync(PaymentMethodForCreationDto paymentMethodDto, string userId);
        Task<PaymentMethodDto> UpdateAsync(Guid id, PaymentMethodForUpdateDto paymentMethodDto, string userId);
        Task DeleteAsync(Guid id, string userId);
    }
}
