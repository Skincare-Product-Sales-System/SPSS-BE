using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.Product;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService) => _productService = productService ?? throw new ArgumentNullException(nameof(productService));

    // GET: api/products/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductWithDetailsDto>.SuccessResponse(product));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductWithDetailsDto>.FailureResponse(ex.Message));
        }
    }

    // GET: api/products?pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ProductDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _productService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ProductDto>>.SuccessResponse(pagedData));
    }

    // POST: api/products
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
        string userId = "123"; // Get the user ID from the request context
        try
        {
            var createdProduct = await _productService.CreateAsync(productDto, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(createdProduct, "Product created successfully"));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ProductDto>.FailureResponse(ex.Message));
        }
    }

    // PUT: api/products/{id}
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
        string userId = "123"; // Get the user ID from the request context
        if (id != productDto.Id)
            return BadRequest(ApiResponse<ProductDto>.FailureResponse("Product ID in URL must match the ID in the body"));

        try
        {
            var updatedProduct = await _productService.UpdateAsync(productDto, userId);
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

    // DELETE: api/products/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            string userId = "123"; // Get the user ID from the request context
            await _productService.DeleteAsync(id, userId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Product deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategoryId(
    Guid categoryId,
    [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
    [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ProductDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _productService.GetByCategoryIdPagedAsync(categoryId, pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ProductDto>>.SuccessResponse(pagedData));
    }
}
