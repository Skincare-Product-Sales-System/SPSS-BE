using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.ProductCategory;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;
[ApiController]
[Route("api/product-categories")]
public class ProductCategoryController : ControllerBase
{
    private readonly IProductCategoryService _productCategoryService;

    public ProductCategoryController(IProductCategoryService productCategoryService) => _productCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var productCategory = await _productCategoryService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductCategoryDto>.SuccessResponse(productCategory));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductCategoryDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<ProductCategoryDto>>.FailureResponse("Invalid pagination parameters", errors));
        }
        var pagedData = await _productCategoryService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ProductCategoryDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProductCategoryForCreationDto productCategoryDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductCategoryDto>.FailureResponse("Invalid product category data", errors));
        }

        try
        {
            var createdProductCategory = await _productCategoryService.CreateAsync(productCategoryDto);
            var response = ApiResponse<ProductCategoryDto>.SuccessResponse(createdProductCategory, "Product category created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdProductCategory.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ProductCategoryDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductCategoryForUpdateDto productCategoryDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductCategoryDto>.FailureResponse("Invalid product category data", errors));
        }

        if (id != productCategoryDto.Id)
            return BadRequest(ApiResponse<ProductCategoryDto>.FailureResponse("Product category ID in URL must match the ID in the body"));

        try
        {
            var updatedProductCategory = await _productCategoryService.UpdateAsync(productCategoryDto);
            return Ok(ApiResponse<ProductCategoryDto>.SuccessResponse(updatedProductCategory, "Product category updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductCategoryDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ProductCategoryDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _productCategoryService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Product category deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
