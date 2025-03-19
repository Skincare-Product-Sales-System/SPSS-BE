using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.PromotionTarget;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/promotion-targets")]
public class PromotionTargetController : ControllerBase
{
    private readonly IPromotionTargetService _promotionTargetService;

    public PromotionTargetController(IPromotionTargetService promotionTargetService)
        => _promotionTargetService = promotionTargetService ?? throw new ArgumentNullException(nameof(promotionTargetService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var promotionTarget = await _promotionTargetService.GetByIdAsync(id);
            return Ok(ApiResponse<PromotionTargetDto>.SuccessResponse(promotionTarget));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PromotionTargetDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<PromotionTargetDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _promotionTargetService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<PromotionTargetDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PromotionTargetForCreationDto promotionTargetDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PromotionTargetDto>.FailureResponse("Invalid promotion target data", errors));
        }

        try
        {
            var promotionTarget = await _promotionTargetService.CreateAsync(promotionTargetDto);
            return CreatedAtAction(nameof(GetById), new { id = promotionTarget.Id }, ApiResponse<PromotionTargetDto>.SuccessResponse(promotionTarget));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PromotionTargetDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PromotionTargetForUpdateDto promotionTargetDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PromotionTargetDto>.FailureResponse("Invalid promotion target data", errors));
        }

        try
        {
            await _promotionTargetService.UpdateAsync(id, promotionTargetDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PromotionTargetDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _promotionTargetService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.FailureResponse(ex.Message));
        }
    }
}