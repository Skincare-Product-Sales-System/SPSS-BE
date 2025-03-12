using AutoMapper;
using BusinessObjects.Dto.Order;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<OrderWithDetailDto> GetByIdAsync(Guid id)
        {
            var order = await _unitOfWork.Orders
                .GetQueryable()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductItem)
                        .ThenInclude(pi => pi.Product)
                            .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderDetails)
                    .ThenInclude(pi => pi.ProductItem)
                        .ThenInclude(pc => pc.ProductConfigurations)
                            .ThenInclude(vo => vo.VariationOption)
                .Include(a => a.Address)
                    .ThenInclude(u => u.User)
                .Include(a => a.Address)
                    .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found.");

            return _mapper.Map<OrderWithDetailDto>(order);
        }

        public async Task<PagedResponse<OrderDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            // Tính tổng số đơn hàng
            var totalCount = await _unitOfWork.Orders.Entities
                .Where(o => !o.IsDeleted)
                .CountAsync();

            // Lấy danh sách đơn hàng theo phân trang
            var orders = await _unitOfWork.Orders.Entities
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductItem)
                        .ThenInclude(pi => pi.Product)
                            .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderDetails)
                    .ThenInclude(pi => pi.ProductItem)
                        .ThenInclude(pc => pc.ProductConfigurations)
                            .ThenInclude(vo => vo.VariationOption)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.CreatedTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Ánh xạ sang DTO sử dụng AutoMapper
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);

            // Trả về kết quả phân trang
            return new PagedResponse<OrderDto>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<OrderDto> CreateAsync(OrderForCreationDto orderDto, Guid userId)
        {
            if (orderDto == null)
                throw new ArgumentNullException(nameof(orderDto), "Order data cannot be null.");

            var order = _mapper.Map<Order>(orderDto);
            order.CreatedTime = DateTimeOffset.UtcNow;
            order.CreatedBy = userId.ToString();
            order.LastUpdatedTime = DateTimeOffset.UtcNow;
            order.LastUpdatedBy = userId.ToString();
            order.IsDeleted = false;

            _unitOfWork.Orders.Add(order);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> UpdateAsync(Guid id, OrderForUpdateDto orderDto, Guid userId)
        {
            if (orderDto == null)
                throw new ArgumentNullException(nameof(orderDto), "Order data cannot be null.");

            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null || order.IsDeleted)
                throw new KeyNotFoundException($"Order with ID {id} not found or has been deleted.");

            order.LastUpdatedTime = DateTimeOffset.UtcNow;
            order.LastUpdatedBy = userId.ToString();

            _mapper.Map(orderDto, order);
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<OrderDto>(order);
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null || order.IsDeleted)
                throw new KeyNotFoundException($"Order with ID {id} not found or has been deleted.");

            order.IsDeleted = true;
            order.DeletedTime = DateTimeOffset.UtcNow;
            order.DeletedBy = userId.ToString();

            _unitOfWork.Orders.Update(order); // Soft delete via update
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
