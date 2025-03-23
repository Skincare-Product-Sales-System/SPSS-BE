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
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductItem)
                        .ThenInclude(pi => pi.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found.");

            // Manually map Order to OrderWithDetailDto
            var orderWithDetailDto = new OrderWithDetailDto
            {
                Id = order.Id,
                Status = order.Status,
                CancelReasonId = order.CancelReasonId,
                OrderTotal = order.OrderTotal,
                CreatedTime = order.CreatedTime,
                PaymentMethodId = order.PaymentMethodId,
                Address = new AddressDto
                {
                    Id = order.Address.Id,
                    IsDefault = order.Address.IsDefault,
                    CustomerName = $"{order.Address.User.SurName} {order.Address.User.LastName}".Trim(),
                    CountryId = order.Address.CountryId,
                    PhoneNumber = order.Address.User.PhoneNumber,
                    CountryName = order.Address.Country.CountryName,
                    StreetNumber = order.Address.StreetNumber,
                    AddressLine1 = order.Address.AddressLine1,
                    AddressLine2 = order.Address.AddressLine2,
                    City = order.Address.City,
                    Ward = order.Address.Ward,
                    PostCode = order.Address.Postcode,
                    Province = order.Address.Province
                },
                StatusChanges = order.StatusChanges.OrderBy(sc => sc.Date)
                .Select(sc => new StatusChangeDto
                {
                    Date = sc.Date,
                    Status = sc.Status
                }).ToList(),
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailDto
                {
                    ProductId = od.ProductItem.ProductId,
                    ProductItemId = od.ProductItemId,
                    ProductImage = od.ProductItem.Product.ProductImages.FirstOrDefault()?.ImageUrl ?? string.Empty,
                    ProductName = od.ProductItem.Product.Name,
                    VariationOptionValues = od.ProductItem.ProductConfigurations.Select(pc => pc.VariationOption.Value).ToList(),
                    Quantity = od.Quantity,
                    Price = od.Price,
                    IsReviewable = od.ProductItem.Reviews == null || !od.ProductItem.Reviews.Any(r => r.UserId == order.UserId && r.ProductItemId == od.ProductItemId && !r.IsDeleted)
                }).ToList()
            };

            return orderWithDetailDto;
        }

        public async Task<PagedResponse<OrderDto>> GetOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize, string? status = null)
        {
            // Filter orders by UserId, IsDeleted, and optional Status
            var query = _unitOfWork.Orders.Entities
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductItem)
                        .ThenInclude(pi => pi.Product)
                            .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductItem)
                        .ThenInclude(pi => pi.ProductConfigurations)
                            .ThenInclude(vo => vo.VariationOption)
                .Where(o => !o.IsDeleted && o.UserId == userId);

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Calculate total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var orders = await query
                .OrderByDescending(o => o.CreatedTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map orders to DTOs
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);

            // Check if the user has already reviewed each product item
            foreach (var orderDto in orderDtos)
            {
                foreach (var orderDetailDto in orderDto.OrderDetails)
                {
                    var hasReviewed = await _unitOfWork.Reviews.Entities
                        .AnyAsync(r => r.UserId == userId && r.ProductItemId == orderDetailDto.ProductItemId && !r.IsDeleted);

                    orderDetailDto.IsReviewable = !hasReviewed;
                }
            }

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
            if (orderDto.OrderDetail == null || !orderDto.OrderDetail.Any())
            {
                throw new ArgumentException("Order details cannot be empty.");
            }

            // Validate User
            var userExists = await _unitOfWork.Users.Entities
                .AnyAsync(u => u.UserId == userId && u.Status == StatusForAccount.Active);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {userId} not found or is inactive.");
            }

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

                    // Validate Order Detail Quantity
                    if (orderDetail.Quantity <= 0)
                    {
                        throw new ArgumentException($"Quantity for ProductItem ID {orderDetail.ProductItemId} must be greater than zero.");
                    }
                }

                // Step 4.1: Remove CartItems associated with the user's product items
                var productItemIds = orderDto.OrderDetail.Select(od => od.ProductItemId).ToList();
                var cartItemsToRemove = await _unitOfWork.CartItems.Entities
                    .Where(ci => ci.UserId == userId && productItemIds.Contains(ci.ProductItemId))
                    .ToListAsync();

                if (cartItemsToRemove.Any())
                {
                    _unitOfWork.CartItems.RemoveRange(cartItemsToRemove);
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

                    if (orderDto.VoucherId != null)
                    {
                        // Find the Voucher
                        var voucher = await _unitOfWork.Vouchers.Entities
                            .FirstOrDefaultAsync(v => v.Id == orderDto.VoucherId);
                        // Apply discount based on voucher discount rate
                        decimal discountAmount = orderTotal * (decimal)(voucher.DiscountRate / 100);
                        orderTotal -= discountAmount;

                        // Decrement the usage limit
                        voucher.UsageLimit--;
                        _unitOfWork.Vouchers.Update(voucher); // Cập nhật voucher trong cơ sở dữ liệu
                    }

                    // Create OrderDetail entity
                    var orderDetailEntity = new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        ProductItemId = orderDetail.ProductItemId,
                        Quantity = orderDetail.Quantity,
                        Price = price,
                    };
                    orderDetailsEntities.Add(orderDetailEntity);
                }

                // Validate Order Total
                if (orderTotal <= 0)
                {
                    throw new ArgumentException("Order total must be greater than zero.");
                }

                // Step 6: Create Order entity
                var orderEntity = new Order
                {
                    Id = Guid.NewGuid(),
                    AddressId = orderDto.AddressId,
                    PaymentMethodId = orderDto.PaymentMethodId,
                    VoucherId = orderDto.VoucherId,
                    Status = StatusForOrder.Processing,
                    OrderTotal = orderTotal,
                    CreatedTime = DateTime.UtcNow,
                    CreatedBy = userId.ToString(),
                    LastUpdatedTime = DateTime.UtcNow,
                    LastUpdatedBy = userId.ToString(),
                    UserId = userId,
                    IsDeleted = false
                };

                // Kiểm tra PaymentMethodId và điều chỉnh trạng thái cùng việc tạo StatusChange
                if (orderEntity.PaymentMethodId == Guid.Parse("ABB33A09-6065-4DC2-A943-51A9DD9DF27E"))
                {
                    // Nếu là "ABB33A09-6065-4DC2-A943-51A9DD9DF27E", trạng thái là Pending và tạo StatusChange
                    orderEntity.Status = StatusForOrder.Processing;

                    // Add Order entity to UnitOfWork
                    _unitOfWork.Orders.Add(orderEntity);

                    // Associate OrderDetails with Order and add them to UnitOfWork
                    foreach (var orderDetailEntity in orderDetailsEntities)
                    {
                        orderDetailEntity.OrderId = orderEntity.Id;
                        _unitOfWork.OrderDetails.Add(orderDetailEntity);
                    }

                    // Create StatusChange entity
                    var statusChangeEntity = new StatusChange
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderEntity.Id,
                        Status = orderEntity.Status,
                        Date = DateTimeOffset.UtcNow,
                    };

                    // Add StatusChange entity to UnitOfWork
                    _unitOfWork.StatusChanges.Add(statusChangeEntity);
                }
                else if (orderEntity.PaymentMethodId == Guid.Parse("354EDA95-5BE5-41BE-ACC3-CFD70188118A"))
                {
                    // Nếu là "354EDA95-5BE5-41BE-ACC3-CFD70188118A", trạng thái là Awaiting Payment và không tạo StatusChange
                    orderEntity.Status = "Awaiting Payment";

                    // Add Order entity to UnitOfWork
                    _unitOfWork.Orders.Add(orderEntity);

                    // Associate OrderDetails with Order and add them to UnitOfWork
                    foreach (var orderDetailEntity in orderDetailsEntities)
                    {
                        orderDetailEntity.OrderId = orderEntity.Id;
                        _unitOfWork.OrderDetails.Add(orderDetailEntity);
                    }
                    var statusChangeEntity = new StatusChange
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderEntity.Id,
                        Status = orderEntity.Status,
                        Date = DateTimeOffset.UtcNow,
                    };
                    // Add StatusChange entity to UnitOfWork
                    _unitOfWork.StatusChanges.Add(statusChangeEntity);
                }

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
                    PaymentMethodId = orderEntity.PaymentMethodId,
                };

                return orderDtoResult;

            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid id, string newStatus, Guid userId, Guid? cancelReasonId = null)
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
                    .Where(od => od.OrderId == id)
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

                // If cancelReasonId is provided, update the order's CancelReasonId
                if (cancelReasonId.HasValue)
                {
                    order.CancelReasonId = cancelReasonId.Value;
                }
            }

            // Handle updating SoldCount for delivered orders
            if (newStatus.Equals(StatusForOrder.Delivered, StringComparison.OrdinalIgnoreCase))
            {
                var orderDetails = await _unitOfWork.OrderDetails.Entities
                    .Where(od => od.OrderId == id)
                    .ToListAsync();

                foreach (var orderDetail in orderDetails)
                {
                    var productItem = await _unitOfWork.ProductItems.Entities
                        .Include(pi => pi.Product)
                        .FirstOrDefaultAsync(pi => pi.Id == orderDetail.ProductItemId);

                    if (productItem == null)
                        throw new KeyNotFoundException($"ProductItem with ID {orderDetail.ProductItemId} not found.");

                    // Update the SoldCount
                    productItem.Product.SoldCount += orderDetail.Quantity;

                    // Update the product
                    _unitOfWork.Products.Update(productItem.Product);
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
            };

            // Add the status change record
            _unitOfWork.StatusChanges.Add(statusChange);

            // Update the order
            _unitOfWork.Orders.Update(order);

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateOrderPaymentMethodAsync(Guid orderId, Guid paymentMethodId, Guid userId)
        {
            // Validate payment method ID
            if (paymentMethodId == Guid.Empty)
                throw new ArgumentNullException(nameof(paymentMethodId), "Payment method cannot be null or empty.");

            // Fetch the order
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null || order.IsDeleted)
                throw new KeyNotFoundException($"Order with ID {orderId} not found or has been deleted.");

            // Fetch the payment method to ensure it exists
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(paymentMethodId);
            if (paymentMethod == null)
                throw new KeyNotFoundException($"Payment method with ID {paymentMethodId} not found.");

            // Update the order's payment method
            order.PaymentMethodId = paymentMethodId;
            order.LastUpdatedTime = DateTimeOffset.UtcNow;
            order.LastUpdatedBy = userId.ToString();

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
