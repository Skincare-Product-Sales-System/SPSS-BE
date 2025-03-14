using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.Promotion; 
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/promotions")] 
public class PromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionController(IPromotionService promotionService) 
        => _promotionService = promotionService ?? throw new ArgumentNullException(nameof(promotionService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var promotion = await _promotionService.GetByIdAsync(id);
            return Ok(ApiResponse<PromotionDto>.SuccessResponse(promotion));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PromotionDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<PromotionDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _promotionService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<PromotionDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PromotionForCreationDto promotionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PromotionDto>.FailureResponse("Invalid promotion data", errors));
        }

        try
        {
            var promotion = await _promotionService.CreateAsync(promotionDto);
            return CreatedAtAction(nameof(GetById), new { id = promotion.Id }, ApiResponse<PromotionDto>.SuccessResponse(promotion));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PromotionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PromotionForUpdateDto promotionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PromotionDto>.FailureResponse("Invalid promotion data", errors));
        }

        try
        {
            await _promotionService.UpdateAsync(id, promotionDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PromotionDto>.FailureResponse(ex.Message));
        }
    }
}