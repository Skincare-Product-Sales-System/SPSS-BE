using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.SkinType; // Adjusted namespace for SkinType DTOs
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/skin-types")] // Route remains the same as per your example
public class SkinTypeController : ControllerBase
{
    private readonly ISkinTypeService _skinTypeService;

    public SkinTypeController(ISkinTypeService skinTypeService) 
        => _skinTypeService = skinTypeService ?? throw new ArgumentNullException(nameof(skinTypeService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var skinType = await _skinTypeService.GetByIdAsync(id);
            return Ok(ApiResponse<SkinTypeDto>.SuccessResponse(skinType));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SkinTypeDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<SkinTypeDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _skinTypeService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<SkinTypeDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SkinTypeForCreationDto skinTypeDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<SkinTypeDto>.FailureResponse("Invalid skin type data", errors));
        }

        try
        {
            var skinType = await _skinTypeService.CreateAsync(skinTypeDto);
            return CreatedAtAction(nameof(GetById), new { id = skinType.Id }, ApiResponse<SkinTypeDto>.SuccessResponse(skinType));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SkinTypeDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SkinTypeForUpdateDto skinTypeDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<SkinTypeDto>.FailureResponse("Invalid skin type data", errors));
        }

        try
        {
            await _skinTypeService.UpdateAsync(id, skinTypeDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SkinTypeDto>.FailureResponse(ex.Message));
        }
    }
}