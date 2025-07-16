using AutoMapper;
using BusinessObjects.Dto.Order;
using BusinessObjects.Models.Dto.Dashboard;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using Shared.Constants;

namespace Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;

        public DashboardService(IUnitOfWork unitOfWork, IMapper mapper, IOrderService orderService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        public async Task<PagedResponse<TotalRevenueDto>> GetTotalRevenueAsync(int pageNumber, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.Orders.Entities
                .Where(o => !o.IsDeleted && o.Status != "Cancelled"); // Exclude cancelled orders

            if (startDate.HasValue)
                query = query.Where(o => o.CreatedTime >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.CreatedTime <= endDate.Value);

            var totalRevenue = await query.SumAsync(o => o.OrderTotal);
            var response = new TotalRevenueDto
            {
                TotalRevenue = totalRevenue
            };

            return new PagedResponse<TotalRevenueDto>
            {
                Items = new List<TotalRevenueDto> { response },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 1
            };
        }
        
        public async Task<int> GetOrderCountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.Orders.Entities
                .Where(o => !o.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(o => o.CreatedTime >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.CreatedTime <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<PagedResponse<TopProductDto>> GetTopSellingProductsAsync(int pageNumber, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.OrderDetails.Entities
                .Include(od => od.ProductItem)
                    .ThenInclude(pi => pi.Product)
                .Where(od => !od.Order.IsDeleted && od.Order.Status != "Cancelled");

            if (startDate.HasValue)
                query = query.Where(od => od.Order.CreatedTime >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(od => od.Order.CreatedTime <= endDate.Value);

            var topProducts = await query
                .GroupBy(od => new { od.ProductItem.ProductId, od.ProductItem.Product.Name })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Price * od.Quantity)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await query
                .GroupBy(od => od.ProductItem.ProductId)
                .CountAsync();

            return new PagedResponse<TopProductDto>
            {
                Items = topProducts,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<OrderStatusDistributionDto>> GetOrderStatusDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.Orders.Entities
                .Where(o => !o.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(o => o.CreatedTime >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.CreatedTime <= endDate.Value);

            var distribution = await query
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusDistributionDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return distribution;
        }
        
        public async Task<PagedResponse<OrderDto>> GetTopPendingOrdersAsync(int topCount)
        {
            var pendingOrdersQuery = _unitOfWork.Orders.Entities
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductItem)
                .ThenInclude(pi => pi.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductItem)
                .ThenInclude(pi => pi.ProductConfigurations)
                .ThenInclude(vo => vo.VariationOption)
                .Where(o => !o.IsDeleted && o.Status == StatusForOrder.Processing)
                .OrderByDescending(o => o.CreatedTime);

            var totalCount = await pendingOrdersQuery.CountAsync();
            var pendingOrders = await pendingOrdersQuery
                .Take(topCount)
                .ToListAsync();

            var orderDtos = _mapper.Map<List<OrderDto>>(pendingOrders);

            return new PagedResponse<OrderDto>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                PageNumber = 1,
                PageSize = topCount
            };
        }
        
        // New methods for financial dashboard
        
        public async Task<FinancialSummaryDto> GetFinancialSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Filter orders by date range and exclude cancelled orders
            var ordersQuery = _unitOfWork.Orders.Entities
                .Include(o => o.Voucher)  // Include voucher information
                .Where(o => !o.IsDeleted && o.Status != "Cancelled");
                
            if (startDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CreatedTime >= startDate.Value);
            if (endDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CreatedTime <= endDate.Value);

            // Get completed orders (Delivered) for revenue calculation
            var completedOrdersQuery = ordersQuery.Where(o => o.Status == StatusForOrder.Delivered);
            
            // Get pending orders count
            var pendingOrdersCount = await ordersQuery
                .Where(o => o.Status == StatusForOrder.Processing || o.Status == StatusForOrder.AwaitingPayment)
                .CountAsync();
            
            // Calculate total revenue from completed orders (this is already net of voucher discounts)
            var totalNetRevenue = await completedOrdersQuery.SumAsync(o => o.OrderTotal);
            
            // Get all order details for completed orders with their order information
            var orderDetails = await _unitOfWork.OrderDetails.Entities
                .Include(od => od.Order)
                    .ThenInclude(o => o.Voucher)
                .Include(od => od.ProductItem)
                .Where(od => !od.Order.IsDeleted && 
                            od.Order.Status == StatusForOrder.Delivered && 
                            (!startDate.HasValue || od.Order.CreatedTime >= startDate.Value) &&
                            (!endDate.HasValue || od.Order.CreatedTime <= endDate.Value))
                .ToListAsync();
            
            // Calculate procurement cost, gross revenue, and discount amount
            decimal totalProcurementCost = 0;
            decimal grossRevenue = 0;
            decimal totalDiscountAmount = 0;
            
            // Group order details by order to calculate per-order metrics
            var orderGroups = orderDetails.GroupBy(od => od.OrderId).ToList();
            
            foreach (var orderGroup in orderGroups)
            {
                // Get the first order detail to access the order info
                var firstDetail = orderGroup.First();
                var order = firstDetail.Order;
                
                // Calculate the raw total for this order (before any discount)
                decimal orderGrossTotal = orderGroup.Sum(od => od.Price * od.Quantity);
                grossRevenue += orderGrossTotal;
                
                // If there's a voucher, calculate the discount amount
                if (order.VoucherId.HasValue && order.Voucher != null)
                {
                    // The discount is the difference between the gross total and the actual order total
                    decimal orderDiscountAmount = orderGrossTotal - order.OrderTotal;
                    totalDiscountAmount += orderDiscountAmount;
                }
                
                // Calculate procurement cost for this order
                foreach (var orderDetail in orderGroup)
                {
                    totalProcurementCost += orderDetail.ProductItem.PurchasePrice * orderDetail.Quantity;
                }
            }
            
            // Calculate profit and profit margin based on net revenue
            var totalProfit = totalNetRevenue - totalProcurementCost;
            var profitMargin = totalNetRevenue > 0 ? (totalProfit / totalNetRevenue) * 100 : 0;
            
            // Get completed order count
            var completedOrderCount = await completedOrdersQuery.CountAsync();

            var inventoryProcurementCost = await GetInventoryProcurementCostAsync();

            return new FinancialSummaryDto
            {
                GrossRevenue = grossRevenue,
                DiscountAmount = totalDiscountAmount,
                TotalRevenue = totalNetRevenue,
                TotalProcurementCost = totalProcurementCost, // Sold products
                InventoryProcurementCost = inventoryProcurementCost, // All inventory
                TotalProfit = totalProfit,
                ProfitMargin = profitMargin,
                CompletedOrderCount = completedOrderCount,
                PendingOrderCount = pendingOrdersCount,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        private async Task<decimal> GetInventoryProcurementCostAsync()
        {
            var productItems = await _unitOfWork.ProductItems.Entities
                .ToListAsync();

            return productItems.Sum(pi => pi.PurchasePrice * pi.QuantityInStock);
        }

        public async Task<PagedResponse<ProductProfitDto>> GetProductProfitAnalysisAsync(int pageNumber, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Create a query for order details of completed orders
            var query = _unitOfWork.OrderDetails.Entities
                .Include(od => od.Order)
                    .ThenInclude(o => o.Voucher)
                .Include(od => od.ProductItem)
                .ThenInclude(pi => pi.Product)
                .Where(od => !od.Order.IsDeleted && 
                             od.Order.Status == StatusForOrder.Delivered);
                
            if (startDate.HasValue)
                query = query.Where(od => od.Order.CreatedTime >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(od => od.Order.CreatedTime <= endDate.Value);
                
            // Group by product and calculate profit metrics
            var productProfits = await query
                .GroupBy(od => new { od.ProductItem.ProductId, od.ProductItem.Product.Name })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(od => od.Quantity),
                    LineItemRevenue = g.Sum(od => od.Price * od.Quantity),
                    PurchasePrice = g.Average(od => od.ProductItem.PurchasePrice)
                })
                .OrderByDescending(p => p.LineItemRevenue)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            // Calculate profit and profit margin for each product
            var productProfitDtos = productProfits.Select(p => 
            {
                var procurementCost = p.PurchasePrice * p.QuantitySold;
                var profit = p.LineItemRevenue - procurementCost;
                var profitMargin = p.LineItemRevenue > 0 ? (profit / p.LineItemRevenue) * 100 : 0;
                
                return new ProductProfitDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    QuantitySold = p.QuantitySold,
                    Revenue = p.LineItemRevenue,
                    ProcurementCost = procurementCost,
                    Profit = profit,
                    ProfitMargin = profitMargin
                };
            }).ToList();
            
            // Get total count for pagination
            var totalCount = await query
                .GroupBy(od => od.ProductItem.ProductId)
                .CountAsync();
                
            return new PagedResponse<ProductProfitDto>
            {
                Items = productProfitDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<MonthlyFinancialReportDto>> GetMonthlyFinancialReportAsync(int year)
        {
            // Create a list to hold the monthly reports
            var monthlyReports = new List<MonthlyFinancialReportDto>();
            
            // Define the date range for the year
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59);
            
            // Get all completed orders for the year with voucher information
            var orders = await _unitOfWork.Orders.Entities
                .Include(o => o.Voucher)
                .Where(o => !o.IsDeleted && 
                           o.Status == StatusForOrder.Delivered &&
                           o.CreatedTime >= startDate &&
                           o.CreatedTime <= endDate)
                .ToListAsync();
                
            // Get all order details for the completed orders
            var orderDetails = await _unitOfWork.OrderDetails.Entities
                .Include(od => od.Order)
                    .ThenInclude(o => o.Voucher)
                .Include(od => od.ProductItem)
                .Where(od => !od.Order.IsDeleted && 
                             od.Order.Status == StatusForOrder.Delivered &&
                             od.Order.CreatedTime >= startDate &&
                             od.Order.CreatedTime <= endDate)
                .ToListAsync();
                
            // Group data by month
            for (int month = 1; month <= 12; month++)
            {
                // Filter orders for this month - handle DateTimeOffset by checking Month on DateTime component
                var monthOrders = orders.Where(o => ((DateTimeOffset)o.CreatedTime).Month == month).ToList();
                var monthOrderDetails = orderDetails.Where(od => ((DateTimeOffset)od.Order.CreatedTime).Month == month).ToList();
                
                // Calculate monthly net revenue (after voucher discounts)
                var monthlyNetRevenue = monthOrders.Sum(o => o.OrderTotal);
                
                // Calculate monthly procurement cost
                decimal monthlyProcurementCost = 0;
                decimal monthlyGrossRevenue = 0;
                decimal monthlyDiscountAmount = 0;
                
                // Group order details by order
                var orderGroups = monthOrderDetails.GroupBy(od => od.OrderId).ToList();
                
                foreach (var orderGroup in orderGroups)
                {
                    // Calculate gross revenue for this order
                    var orderGrossRevenue = orderGroup.Sum(od => od.Price * od.Quantity);
                    monthlyGrossRevenue += orderGrossRevenue;
                    
                    // Get the first order detail to access the order info
                    var firstDetail = orderGroup.First();
                    var order = firstDetail.Order;
                    
                    // If there's a voucher, calculate the discount amount
                    if (order.VoucherId.HasValue && order.Voucher != null)
                    {
                        decimal orderDiscountAmount = orderGrossRevenue - order.OrderTotal;
                        monthlyDiscountAmount += orderDiscountAmount;
                    }
                    
                    // Calculate procurement cost
                    foreach (var orderDetail in orderGroup)
                    {
                        var purchasePrice = orderDetail.ProductItem.PurchasePrice;
                        monthlyProcurementCost += purchasePrice * orderDetail.Quantity;
                    }
                }
                
                // Calculate profit and profit margin based on net revenue
                var monthlyProfit = monthlyNetRevenue - monthlyProcurementCost;
                var monthlyProfitMargin = monthlyNetRevenue > 0 ? (monthlyProfit / monthlyNetRevenue) * 100 : 0;
                
                // Create the monthly report with both gross and net revenue
                monthlyReports.Add(new MonthlyFinancialReportDto
                {
                    Year = year,
                    Month = month,
                    GrossRevenue = monthlyGrossRevenue,
                    DiscountAmount = monthlyDiscountAmount,
                    Revenue = monthlyNetRevenue,
                    ProcurementCost = monthlyProcurementCost,
                    Profit = monthlyProfit,
                    ProfitMargin = monthlyProfitMargin,
                    OrderCount = monthOrders.Count
                });
            }
            
            return monthlyReports;
        }
    }
}