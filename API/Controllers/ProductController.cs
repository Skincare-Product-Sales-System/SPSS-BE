using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.Product;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;
[ApiController]
[Microsoft.AspNetCore.Components.Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService) => _productService = productService ?? throw new ArgumentNullException(nameof(productService));
   
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<ProductDto>>.FailureResponse("Invalid pagination parameters", errors));
        }
        var pagedData = await _productService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ProductDto>>.SuccessResponse(pagedData));
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProductForCreationDto productDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductDto>.FailureResponse("Invalid product data", errors));
        }

        try
        {
            var createdProduct = await _productService.CreateAsync(productDto);
            var response = ApiResponse<ProductDto>.SuccessResponse(createdProduct, "Product created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ProductDto>.FailureResponse(ex.Message));
        }
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)] 
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductForUpdateDto productDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductDto>.FailureResponse("Invalid product data", errors));
        }

        if (id != productDto.Id)
            return BadRequest(ApiResponse<ProductDto>.FailureResponse("Product ID in URL must match the ID in the body"));

        try
        {
            var updatedProduct = await _productService.UpdateAsync(productDto);
            return Ok(ApiResponse<ProductDto>.SuccessResponse(updatedProduct, "Product updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ProductDto>.FailureResponse(ex.Message));
        }
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)] 
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Product deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
    
}