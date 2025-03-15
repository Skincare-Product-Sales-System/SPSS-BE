using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.Variation;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/variations")]
public class VariationController : ControllerBase
{
    private readonly IVariationService _variationService;

    public VariationController(IVariationService variationService) =>
        _variationService = variationService ?? throw new ArgumentNullException(nameof(variationService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var variation = await _variationService.GetByIdAsync(id);
            return Ok(ApiResponse<VariationDto>.SuccessResponse(variation));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VariationDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<VariationDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _variationService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<VariationDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] VariationForCreationDto variationDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<VariationDto>.FailureResponse("Invalid variation data", errors));
        }
        Guid? userId = HttpContext.Items["UserId"] as Guid?;
        try
        {
            var createdVariation = await _variationService.CreateAsync(variationDto, userId);
            var response = ApiResponse<VariationDto>.SuccessResponse(createdVariation, "Variation created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdVariation.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<VariationDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] VariationForUpdateDto variationDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<VariationDto>.FailureResponse("Invalid variation data", errors));
        }
        Guid? userId = HttpContext.Items["UserId"] as Guid?;
        try
        {
            var updatedVariation = await _variationService.UpdateAsync(id, variationDto, userId);
            return Ok(ApiResponse<VariationDto>.SuccessResponse(updatedVariation, "Variation updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VariationDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<VariationDto>.FailureResponse(ex.Message));
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
            await _variationService.DeleteAsync(id, userId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Variation deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
