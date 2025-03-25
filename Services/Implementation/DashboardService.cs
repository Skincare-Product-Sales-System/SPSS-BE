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
        //
        // public async Task<List<RevenueTrendDto>> GetRevenueTrendAsync(DateTime startDate, DateTime endDate, string granularity = "daily")
        // {
        //     var query = _unitOfWork.Orders.Entities
        //         .Where(o => !o.IsDeleted && o.Status != "Cancelled"
        //                  && o.CreatedTime >= startDate && o.CreatedTime <= endDate);
        //
        //     var revenueTrend = granularity.ToLower() switch
        //     {
        //         "monthly" => await query
        //             .GroupBy(o => new { o.CreatedTime.Year, o.CreatedTime.Month })
        //             .Select(g => new RevenueTrendDto
        //             {
        //                 Date = new DateTime(g.Key.Year, g.Key.Month, 1),
        //                 Revenue = g.Sum(o => o.OrderTotal)
        //             })
        //             .OrderBy(t => t.Date)
        //             .ToListAsync(),
        //
        //         _ => await query // Default to daily
        //             .GroupBy(o => o.CreatedTime.Date)
        //             .Select(g => new RevenueTrendDto
        //             {
        //                 Date = g.Key,
        //                 Revenue = g.Sum(o => o.OrderTotal)
        //             })
        //             .OrderBy(t => t.Date)
        //             .ToListAsync()
        //     };
        //
        //     return revenueTrend;
        // }

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
    }
}