using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.VariationOption;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/variation-options")]
public class VariationOptionController : ControllerBase
{
    private readonly IVariationOptionService _variationOptionService;

    public VariationOptionController(IVariationOptionService variationOptionService) =>
        _variationOptionService = variationOptionService ?? throw new ArgumentNullException(nameof(variationOptionService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var variationOption = await _variationOptionService.GetByIdAsync(id);
            return Ok(ApiResponse<VariationOptionDto>.SuccessResponse(variationOption));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VariationOptionDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<VariationOptionDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _variationOptionService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<VariationOptionDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] VariationOptionForCreationDto variationOptionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<VariationOptionDto>.FailureResponse("Invalid variation option data", errors));
        }
        string userId = "System";
        try
        {
            var createdVariationOption = await _variationOptionService.CreateAsync(variationOptionDto, userId);
            var response = ApiResponse<VariationOptionDto>.SuccessResponse(createdVariationOption, "Variation option created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdVariationOption.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<VariationOptionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] VariationOptionForUpdateDto variationOptionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<VariationOptionDto>.FailureResponse("Invalid variation option data", errors));
        }
        string userId = "System";
        try
        {
            var updatedVariationOption = await _variationOptionService.UpdateAsync(id, variationOptionDto, userId);
            return Ok(ApiResponse<VariationOptionDto>.SuccessResponse(updatedVariationOption, "Variation option updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VariationOptionDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<VariationOptionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            string userId = "System";
            await _variationOptionService.DeleteAsync(id, userId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Variation option deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
