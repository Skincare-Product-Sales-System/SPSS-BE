using BusinessObjects.Dto.Order;
using BusinessObjects.Models;
using Services.Response;

namespace Services.Interface
{
    public interface IOrderService
    {
        Task<OrderWithDetailDto> GetByIdAsync(Guid id);
        Task<PagedResponse<OrderDto>> GetOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize, string? status = null);
        Task<int> GetTotalOrdersByUserIdAsync(Guid userId);
        Task<PagedResponse<OrderDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<OrderDto> CreateAsync(OrderForCreationDto orderDto, Guid userId);
        Task<bool> UpdateOrderPaymentMethodAsync(Guid orderId, Guid paymentMethodId, Guid userId);
        Task DeleteAsync(Guid id, Guid userId);
        Task<bool> UpdateOrderStatusAsync(Guid id, string newStatus, Guid userId, Guid? cancelReasonId = null);
        Task<bool> UpdateOrderAddressAsync(Guid id, Guid newAddressId, Guid userId);
       
    }
}
