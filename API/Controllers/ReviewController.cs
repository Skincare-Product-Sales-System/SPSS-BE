using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.Review;
using Services.Dto.Api;
using Services.Response;
using API.Extensions;
using BusinessObjects.Dto.Account;

namespace API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService) =>
        _reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));

    [HttpGet("user")]
    public async Task<IActionResult> GetByUserId(
    [Range(1, int.MaxValue)] int pageNumber = 1,
    [Range(1, 100)] int pageSize = 10)
    {
        // Kiểm tra tính hợp lệ của các tham số
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ReviewDto>>.FailureResponse("Invalid pagination parameters", errors));
        }
        Guid? userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
        }
        // Gọi service để lấy dữ liệu
        var pagedReviews = await _reviewService.GetPagedByUserIdAsync(userId.Value, pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ReviewDto>>.SuccessResponse(pagedReviews));
    }

    [HttpGet("user/total-reviews")]
    public async Task<IActionResult> GetTotalReviewsByUserId()
    {
        try
        {
            // Lấy UserId từ HttpContext
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return Unauthorized(ApiResponse<int>.FailureResponse("Unauthorized access", new List<string> { "User ID not found in context." }));
            }

            // Gọi service để đếm tổng số review
            var totalReviews = await _reviewService.GetTotalReviewsByUserIdAsync(userId.Value);
            return Ok(ApiResponse<int>.SuccessResponse(totalReviews));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<int>.FailureResponse("Failed to retrieve total reviews", new List<string> { ex.Message }));
        }
    }

    // Lấy danh sách đánh giá phân trang theo sản phẩm
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProductId(
        Guid productId,
        [Range(1, int.MaxValue)] int pageNumber = 1,
        [Range(1, 100)] int pageSize = 10,
        [FromQuery] int? ratingFilter = null) // Thêm tham số lọc rating
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ReviewForProductQueryDto>>.FailureResponse("Invalid parameters", errors));
        }

        // Kiểm tra ratingFilter hợp lệ (1–5 hoặc null)
        if (ratingFilter.HasValue && (ratingFilter < 1 || ratingFilter > 5))
        {
            return BadRequest(ApiResponse<PagedResponse<ReviewForProductQueryDto>>.FailureResponse("Invalid rating filter. Rating must be between 1 and 5."));
        }

        // Lấy dữ liệu từ service
        var pagedData = await _reviewService.GetReviewsByProductIdAsync(productId, pageNumber, pageSize, ratingFilter);

        // Trả về kết quả
        return Ok(ApiResponse<PagedResponse<ReviewForProductQueryDto>>.SuccessResponse(pagedData));
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [Range(1, int.MaxValue)] int pageNumber = 1,
        [Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ReviewDto>>.FailureResponse("Invalid pagination parameters", errors));
        }
        var pagedData = await _reviewService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ReviewDto>>.SuccessResponse(pagedData));
    }
    [CustomAuthorize("Customer")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ReviewForCreationDto reviewDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ReviewForCreationDto>.FailureResponse("Invalid review data", errors));
        }

        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
            }
            var createdReview = await _reviewService.CreateAsync(userId.Value, reviewDto);
            return Ok(ApiResponse<ReviewForCreationDto>.SuccessResponse(createdReview, "Review created successfully"));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ReviewDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Customer")]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ReviewForUpdateDto reviewDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ReviewDto>.FailureResponse("Invalid review data", errors));
        }

        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
            }
            var updatedReview = await _reviewService.UpdateAsync(userId.Value, reviewDto, id);
            return Ok(ApiResponse<ReviewDto>.SuccessResponse(updatedReview, "Review updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ReviewDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ReviewDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Manager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
            }
            await _reviewService.DeleteAsync(userId.Value, id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Review deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
