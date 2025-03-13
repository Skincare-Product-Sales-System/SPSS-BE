using AutoMapper;
using BusinessObjects.Dto.Order;
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

                // Step 6: Map and create the Order entity from DTO
                var orderEntity = _mapper.Map<Order>(orderDto);
                orderEntity.Id = Guid.NewGuid();  // Generate new ID for Order
                orderEntity.Status = StatusForOrder.Pending; // Set the default status as Pending
                orderEntity.CreatedTime = DateTime.UtcNow;
                orderEntity.CreatedBy = userId.ToString();
                orderEntity.LastUpdatedTime = DateTime.UtcNow;
                orderEntity.LastUpdatedBy = userId.ToString();
                orderEntity.UserId = userId;
                orderEntity.IsDeleted = false;

                // Step 5: Calculate Order Total based on Price and Quantity
                decimal orderTotal = 0;
                foreach (var orderDetail in orderDto.OrderDetail)
                {
                    // Find the ProductItem from the database
                    var productItem = await _unitOfWork.ProductItems.Entities
                        .FirstOrDefaultAsync(pi => pi.Id == orderDetail.ProductItemId);

                    if (productItem == null)
                        throw new ArgumentException($"Product item with ID {orderDetail.ProductItemId} does not exist.");

                    // Ensure that we get the correct price from the ProductItem
                    decimal price = productItem.Price;
                    orderTotal += price * orderDetail.Quantity;

                    // Update the QuantityInStock of the ProductItem after the order is placed
                    productItem.QuantityInStock -= orderDetail.Quantity;
                    if (productItem.QuantityInStock < 0)
                    {
                        throw new ArgumentException($"Not enough stock for ProductItem ID {orderDetail.ProductItemId}. Available stock: {productItem.QuantityInStock}");
                    }

                    _unitOfWork.ProductItems.Update(productItem); // Update the ProductItem with the new quantity

                    // Map to OrderDetail entity and set the price from the ProductItem
                    var orderDetailEntity = _mapper.Map<OrderDetail>(orderDetail);
                    orderDetailEntity.Price = price;  // Ensure the price is set from the ProductItem
                    orderDetailEntity.Id = Guid.NewGuid();
                    orderDetailEntity.CreatedTime = DateTime.UtcNow;
                    orderDetailEntity.CreatedBy = userId.ToString();
                    orderDetailEntity.LastUpdatedTime = DateTime.UtcNow;
                    orderDetailEntity.LastUpdatedBy = userId.ToString();
                    orderDetailEntity.IsDeleted = false;
                    orderDetailEntity.OrderId = orderEntity.Id;  // Set the correct OrderId from the orderEntity
                    _unitOfWork.OrderDetails.Add(orderDetailEntity); // Add the OrderDetail entity to UnitOfWork
                }

                // Step 7: Add the Order entity to the UnitOfWork first (before OrderDetails)
                _unitOfWork.Orders.Add(orderEntity);

                // Step 9: Create StatusChange for the new Order
                var statusChangeDto = new StatusChangeForCreationDto
                {
                    Date = DateTimeOffset.UtcNow,
                    Status = orderEntity.Status,
                    OrderId = orderEntity.Id
                };

                // Map StatusChange entity from the DTO
                var statusChangeEntity = _mapper.Map<StatusChange>(statusChangeDto);
                statusChangeEntity.Id = Guid.NewGuid();
                statusChangeEntity.CreatedTime = DateTime.UtcNow;
                statusChangeEntity.CreatedBy = userId.ToString();
                statusChangeEntity.LastUpdatedTime = DateTime.UtcNow;
                statusChangeEntity.LastUpdatedBy = userId.ToString();
                statusChangeEntity.IsDeleted = false;
                _unitOfWork.StatusChanges.Add(statusChangeEntity); // Add the StatusChange entity to UnitOfWork

                // Step 9: Save Changes to the Database
                await _unitOfWork.SaveChangesAsync();

                // Step 10: Commit the Transaction
                await _unitOfWork.CommitTransactionAsync();

                // Step 11: Map and return the OrderDto
                return _mapper.Map<OrderDto>(orderEntity);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
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
