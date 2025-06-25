using BusinessObjects.Models.Dto.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;
using System.ComponentModel.DataAnnotations;
using API.Extensions;

namespace API.Controllers
{
    [ApiController]
    [Route("api/financial-dashboard")]
    public class FinancialDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public FinancialDashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [CustomAuthorize("Manager")]
        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<FinancialSummaryDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetFinancialSummary([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var summary = await _dashboardService.GetFinancialSummaryAsync(startDate, endDate);
                return Ok(ApiResponse<FinancialSummaryDto>.SuccessResponse(summary, "Financial summary retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Error retrieving financial summary", new List<string> { ex.Message }));
            }
        }

        [CustomAuthorize("Manager")]
        [HttpGet("product-profit")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PagedResponse<ProductProfitDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetProductProfitAnalysis(
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<object>.FailureResponse("Invalid pagination parameters", errors));
                }

                var productProfits = await _dashboardService.GetProductProfitAnalysisAsync(pageNumber, pageSize, startDate, endDate);
                return Ok(ApiResponse<PagedResponse<ProductProfitDto>>.SuccessResponse(productProfits, "Product profit analysis retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Error retrieving product profit analysis", new List<string> { ex.Message }));
            }
        }

        [CustomAuthorize("Manager")]
        [HttpGet("monthly-report/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<MonthlyFinancialReportDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetMonthlyFinancialReport([FromRoute, Range(2000, 2100)] int year)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<object>.FailureResponse("Invalid year parameter", errors));
                }

                var monthlyReport = await _dashboardService.GetMonthlyFinancialReportAsync(year);
                return Ok(ApiResponse<List<MonthlyFinancialReportDto>>.SuccessResponse(monthlyReport, $"Monthly financial report for {year} retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Error retrieving monthly financial report", new List<string> { ex.Message }));
            }
        }
        
        [CustomAuthorize("Manager")]
        [HttpGet("all-financial-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetAllFinancialData([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Set default date range if not provided
                if (!startDate.HasValue)
                    startDate = DateTime.Today.AddMonths(-1);
                
                if (!endDate.HasValue)
                    endDate = DateTime.Today;
                
                // Get financial summary
                var summary = await _dashboardService.GetFinancialSummaryAsync(startDate, endDate);
                
                // Get top 10 most profitable products
                var topProducts = await _dashboardService.GetProductProfitAnalysisAsync(1, 10, startDate, endDate);
                
                // Get monthly financial report for current year
                var currentYear = DateTime.Today.Year;
                var monthlyReport = await _dashboardService.GetMonthlyFinancialReportAsync(currentYear);
                
                // Get order status distribution
                var orderStatusDistribution = await _dashboardService.GetOrderStatusDistributionAsync(startDate, endDate);
                
                // Combine all data into a single response
                var combinedData = new
                {
                    FinancialSummary = summary,
                    TopProfitableProducts = topProducts.Items,
                    MonthlyReports = monthlyReport,
                    OrderStatusDistribution = orderStatusDistribution
                };
                
                return Ok(ApiResponse<object>.SuccessResponse(combinedData, "Financial dashboard data retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Error retrieving financial dashboard data", new List<string> { ex.Message }));
            }
        }
    }
}