using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.Product;
using BusinessObjects.Models.Dto.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

[ApiController]
[Route("api/dashboards")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IProductService _productService;
    public DashboardController(IDashboardService dashboardService, IProductService productService)
    {
        _dashboardService = dashboardService;
        _productService = productService;
    }

    [HttpGet("total-revenue")]
    public async Task<IActionResult> GetTotalRevenue(
        [Range(1, int.MaxValue)] int pageNumber = 1,
        [Range(1, 100)] int pageSize = 10,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<TotalRevenueDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _dashboardService.GetTotalRevenueAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<TotalRevenueDto>>.SuccessResponse(pagedData));
    }

    // [HttpGet("revenue-trend")]
    // public async Task<IActionResult> GetRevenueTrend(DateTime startDate, DateTime endDate, string granularity = "daily")
    // {
    //     var trend = await _dashboardService.GetRevenueTrendAsync(startDate, endDate, granularity);
    //     return Ok(trend);
    // }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopSellingProducts(int pageNumber = 1, int pageSize = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        var products = await _dashboardService.GetTopSellingProductsAsync(pageNumber, pageSize, startDate, endDate);
        return Ok(products);
    }
    
    // Add this method to the ProductController
    [HttpGet("best-sellers")]
    public async Task<IActionResult> GetBestSellers(
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ProductDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _productService.GetBestSellerAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ProductDto>>.SuccessResponse(pagedData));
    }

}