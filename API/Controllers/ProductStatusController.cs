using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.ProductStatus;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/product-statuses")]
public class ProductStatusController : ControllerBase
{
    private readonly IProductStatusService _productStatusService;

    public ProductStatusController(IProductStatusService productStatusService) =>
        _productStatusService = productStatusService ?? throw new ArgumentNullException(nameof(productStatusService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var productStatus = await _productStatusService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductStatusDto>.SuccessResponse(productStatus));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductStatusDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<ProductStatusDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _productStatusService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ProductStatusDto>>.SuccessResponse(pagedData));
    }

    [CustomAuthorize("Manager")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProductStatusForCreationDto productStatusDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductStatusDto>.FailureResponse("Invalid product status data", errors));
        }

        try
        {
            var createdProductStatus = await _productStatusService.CreateAsync(productStatusDto);
            var response = ApiResponse<ProductStatusDto>.SuccessResponse(createdProductStatus, "Product status created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdProductStatus.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ProductStatusDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Manager")]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductStatusForUpdateDto productStatusDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductStatusDto>.FailureResponse("Invalid product status data", errors));
        }

        try
        {
            var updatedProductStatus = await _productStatusService.UpdateAsync(id, productStatusDto);
            return Ok(ApiResponse<ProductStatusDto>.SuccessResponse(updatedProductStatus, "Product status updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductStatusDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ProductStatusDto>.FailureResponse(ex.Message));
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
            await _productStatusService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Product status deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
