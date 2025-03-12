using BusinessObjects.Dto.Order;
using Services.Response;

namespace Services.Interface
{
    public interface IOrderService
    {
        Task<OrderWithDetailDto> GetByIdAsync(Guid id);
        Task<PagedResponse<OrderDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<OrderDto> CreateAsync(OrderForCreationDto orderDto, Guid userId);
        Task<OrderDto> UpdateAsync(Guid id, OrderForUpdateDto orderDto, Guid userId);
        Task DeleteAsync(Guid id, Guid userId);
    }
}
