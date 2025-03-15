using BusinessObjects.Dto.Brand;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Response;
using System.ComponentModel.DataAnnotations;
using Services.Dto.Api;

namespace API.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var brand = await _brandService.GetByIdAsync(id);
            return Ok(ApiResponse<BrandDto>.SuccessResponse(brand));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<BrandDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([Range(1, int.MaxValue)] int pageNumber = 1, [Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<BrandDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedBrands = await _brandService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<BrandDto>>.SuccessResponse(pagedBrands));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] BrandForCreationDto brandDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<BrandDto>.FailureResponse("Invalid brand data", errors));
        }

        try
        {
            var createdBrand = await _brandService.CreateAsync(brandDto);
            return CreatedAtAction(nameof(GetById), new { id = createdBrand.Id }, ApiResponse<BrandDto>.SuccessResponse(createdBrand));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BrandDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] BrandForUpdateDto brandDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<BrandDto>.FailureResponse("Invalid brand data", errors));
        }

        try
        {
            await _brandService.UpdateAsync(id, brandDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<BrandDto>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BrandDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _brandService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<BrandDto>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BrandDto>.FailureResponse(ex.Message));
        }
    }
}
