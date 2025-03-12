using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.Review;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService) =>
        _reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));

    // Lấy danh sách đánh giá phân trang theo sản phẩm
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProductId(
        Guid productId,
        [Range(1, int.MaxValue)] int pageNumber = 1,
        [Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ReviewForProductQueryDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _reviewService.GetReviewsByProductIdAsync(productId, pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ReviewForProductQueryDto>>.SuccessResponse(pagedData));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var review = await _reviewService.GetByIdAsync(id);
            return Ok(ApiResponse<ReviewDto>.SuccessResponse(review));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ReviewDto>.FailureResponse(ex.Message));
        }
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ReviewForCreationDto reviewDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ReviewDto>.FailureResponse("Invalid review data", errors));
        }

        try
        {
            var createdReview = await _reviewService.CreateAsync(reviewDto);
            var response = ApiResponse<ReviewDto>.SuccessResponse(createdReview, "Review created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdReview.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ReviewDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
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

        if (id != reviewDto.Id)
            return BadRequest(ApiResponse<ReviewDto>.FailureResponse("Review ID in URL must match the ID in the body"));

        try
        {
            var updatedReview = await _reviewService.UpdateAsync(reviewDto);
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

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _reviewService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Review deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
