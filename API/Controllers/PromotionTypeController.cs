using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.PromotionType;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/promotion-types")]
public class PromotionTypeController : ControllerBase
{
    private readonly IPromotionTypeService _promotionTypeService;

    public PromotionTypeController(IPromotionTypeService promotionTypeService) =>
        _promotionTypeService = promotionTypeService ?? throw new ArgumentNullException(nameof(promotionTypeService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var promotionType = await _promotionTypeService.GetByIdAsync(id);
            return Ok(ApiResponse<PromotionTypeDto>.SuccessResponse(promotionType));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PromotionTypeDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var promotionTypes = await _promotionTypeService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PromotionTypeDto>>.SuccessResponse(promotionTypes));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<PromotionTypeDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PromotionTypeForCreationDto promotionTypeDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PromotionTypeDto>.FailureResponse("Invalid promotion type data", errors));
        }
        Guid? userId = HttpContext.Items["UserId"] as Guid?;
        try
        {
            var createdPromotionType = await _promotionTypeService.CreateAsync(promotionTypeDto, userId.ToString());
            var response = ApiResponse<PromotionTypeDto>.SuccessResponse(createdPromotionType, "Promotion type created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdPromotionType.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<PromotionTypeDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PromotionTypeForUpdateDto promotionTypeDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PromotionTypeDto>.FailureResponse("Invalid promotion type data", errors));
        }
        Guid? userId = HttpContext.Items["UserId"] as Guid?;
        try
        {
            var updatedPromotionType = await _promotionTypeService.UpdateAsync(id, promotionTypeDto, userId.ToString());
            return Ok(ApiResponse<PromotionTypeDto>.SuccessResponse(updatedPromotionType, "Promotion type updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PromotionTypeDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<PromotionTypeDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            await _promotionTypeService.DeleteAsync(id, userId.ToString());
            return Ok(ApiResponse<object>.SuccessResponse(null, "Promotion type deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
