using AutoMapper;
using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.Order;
using BusinessObjects.Dto.OrderDetail;
using BusinessObjects.Dto.StatusChange;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using Shared.Constants;

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
                .Include(os => os.StatusChanges)
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

        public async Task<PagedResponse<OrderDto>> GetOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var totalCount = await _unitOfWork.Orders.Entities
                .Where(o => !o.IsDeleted && o.UserId == userId)
                .CountAsync();

            var orders = await _unitOfWork.Orders.Entities
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductItem)
                        .ThenInclude(pi => pi.Product)
                            .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderDetails)
                    .ThenInclude(pi => pi.ProductItem)
                        .ThenInclude(pc => pc.ProductConfigurations)
                            .ThenInclude(vo => vo.VariationOption)
                .Where(o => !o.IsDeleted && o.UserId == userId) 
                .OrderByDescending(o => o.CreatedTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);

            return new PagedResponse<OrderDto>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
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
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Step 1: Validate Address
                var addressExists = await _unitOfWork.Addresses.Entities
                    .AnyAsync(a => a.Id == orderDto.AddressId);
                if (!addressExists)
                {
                    throw new ArgumentException($"Address with ID {orderDto.AddressId} not found.");
                }

                // Step 2: Validate Payment Method
                var paymentMethodExists = await _unitOfWork.PaymentMethods.Entities
                    .AnyAsync(pm => pm.Id == orderDto.PaymentMethodId);
                if (!paymentMethodExists)
                {
                    throw new ArgumentException($"Payment method with ID {orderDto.PaymentMethodId} not found.");
                }

                // Step 3: Validate Voucher (if provided)
                if (orderDto.VoucherId.HasValue)
                {
                    var voucherExists = await _unitOfWork.Vouchers.Entities
                        .AnyAsync(v => v.Id == orderDto.VoucherId);
                    if (!voucherExists)
                    {
                        throw new ArgumentException($"Voucher with ID {orderDto.VoucherId} not found.");
                    }
                }

                // Step 4: Validate Product Items (ensure each ProductItem exists)
                foreach (var orderDetail in orderDto.OrderDetail)
                {
                    var productItemExists = await _unitOfWork.ProductItems.Entities
                        .AnyAsync(pi => pi.Id == orderDetail.ProductItemId);
                    if (!productItemExists)
                    {
                        throw new ArgumentException($"Product item with ID {orderDetail.ProductItemId} not found.");
                    }
                }

                // Step 5: Calculate Order Total and update stock
                decimal orderTotal = 0;
                var orderDetailsEntities = new List<OrderDetail>();
                foreach (var orderDetail in orderDto.OrderDetail)
                {
                    // Find the ProductItem
                    var productItem = await _unitOfWork.ProductItems.Entities
                        .FirstOrDefaultAsync(pi => pi.Id == orderDetail.ProductItemId);

                    if (productItem == null)
                        throw new ArgumentException($"Product item with ID {orderDetail.ProductItemId} does not exist.");

                    // Ensure sufficient stock
                    if (productItem.QuantityInStock < orderDetail.Quantity)
                    {
                        throw new ArgumentException($"Not enough stock for ProductItem ID {orderDetail.ProductItemId}. Available stock: {productItem.QuantityInStock}");
                    }

                    // Update stock
                    productItem.QuantityInStock -= orderDetail.Quantity;
                    _unitOfWork.ProductItems.Update(productItem);

                    // Calculate total
                    decimal price = productItem.Price;
                    orderTotal += price * orderDetail.Quantity;

                    // Create OrderDetail entity
                    var orderDetailEntity = new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        ProductItemId = orderDetail.ProductItemId,
                        Quantity = orderDetail.Quantity,
                        Price = price,
                        CreatedTime = DateTime.UtcNow,
                        CreatedBy = userId.ToString(),
                        LastUpdatedTime = DateTime.UtcNow,
                        LastUpdatedBy = userId.ToString(),
                        IsDeleted = false
                    };
                    orderDetailsEntities.Add(orderDetailEntity);
                }

                // Step 6: Create Order entity
                var orderEntity = new Order
                {
                    Id = Guid.NewGuid(),
                    AddressId = orderDto.AddressId,
                    PaymentMethodId = orderDto.PaymentMethodId,
                    VoucherId = orderDto.VoucherId,
                    Status = StatusForOrder.Pending,
                    OrderTotal = orderTotal,
                    CreatedTime = DateTime.UtcNow,
                    CreatedBy = userId.ToString(),
                    LastUpdatedTime = DateTime.UtcNow,
                    LastUpdatedBy = userId.ToString(),
                    UserId = userId,
                    IsDeleted = false
                };

                // Add Order entity to UnitOfWork
                _unitOfWork.Orders.Add(orderEntity);

                // Associate OrderDetails with Order and add them to UnitOfWork
                foreach (var orderDetailEntity in orderDetailsEntities)
                {
                    orderDetailEntity.OrderId = orderEntity.Id;
                    _unitOfWork.OrderDetails.Add(orderDetailEntity);
                }

                // Step 7: Create StatusChange entity
                var statusChangeEntity = new StatusChange
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderEntity.Id,
                    Status = orderEntity.Status,
                    Date = DateTimeOffset.UtcNow,
                    CreatedTime = DateTime.UtcNow,
                    CreatedBy = userId.ToString(),
                    LastUpdatedTime = DateTime.UtcNow,
                    LastUpdatedBy = userId.ToString(),
                    IsDeleted = false
                };

                // Add StatusChange entity to UnitOfWork
                _unitOfWork.StatusChanges.Add(statusChangeEntity);

                // Step 8: Save changes
                await _unitOfWork.SaveChangesAsync();

                // Step 9: Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Step 10: Map Order entity to OrderDto
                var orderDtoResult = new OrderDto
                {
                    Id = orderEntity.Id,
                    Status = orderEntity.Status,
                    OrderTotal = orderEntity.OrderTotal,
                    CreatedTime = orderEntity.CreatedTime,
                };

                return orderDtoResult;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid id, string newStatus, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentNullException(nameof(newStatus), "Order status cannot be null or empty.");

            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null || order.IsDeleted)
                throw new KeyNotFoundException($"Order with ID {id} not found or has been deleted.");

            // Handle restocking for cancelled orders
            if (newStatus.Equals(StatusForOrder.Cancelled, StringComparison.OrdinalIgnoreCase))
            {
                var orderDetails = await _unitOfWork.OrderDetails.Entities
                    .Where(od => od.OrderId == id && !od.IsDeleted)
                    .ToListAsync();

                foreach (var orderDetail in orderDetails)
                {
                    var productItem = await _unitOfWork.ProductItems.Entities
                        .FirstOrDefaultAsync(pi => pi.Id == orderDetail.ProductItemId);

                    if (productItem == null)
                        throw new KeyNotFoundException($"ProductItem with ID {orderDetail.ProductItemId} not found.");

                    // Restock the quantity
                    productItem.QuantityInStock += orderDetail.Quantity;

                    // Update the product item
                    _unitOfWork.ProductItems.Update(productItem);
                }
            }

            // Update the order's status
            order.Status = newStatus;
            order.LastUpdatedTime = DateTimeOffset.UtcNow;
            order.LastUpdatedBy = userId.ToString();

            // Create a status change record
            var statusChange = new StatusChange
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = order.Status,
                Date = DateTimeOffset.UtcNow,
                CreatedTime = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                LastUpdatedTime = DateTime.UtcNow,
                LastUpdatedBy = userId.ToString(),
                IsDeleted = false
            };

            // Add the status change record
            _unitOfWork.StatusChanges.Add(statusChange);

            // Update the order
            _unitOfWork.Orders.Update(order);

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateOrderAddressAsync(Guid id, Guid newAddressId, Guid userId)
        {

            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null || order.IsDeleted)
                throw new KeyNotFoundException($"Order with ID {id} not found or has been deleted.");

            order.AddressId = newAddressId;
            order.LastUpdatedTime = DateTimeOffset.UtcNow;
            order.LastUpdatedBy = userId.ToString();

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return true;
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
