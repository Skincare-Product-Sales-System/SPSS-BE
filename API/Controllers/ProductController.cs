using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.Product;
using Services.Dto.Api;
using Services.Response;
using API.Extensions;

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

    [HttpGet("by-skin-type/{skinTypeId:guid}")]
    public async Task<IActionResult> GetBySkinType(
    Guid skinTypeId,
    [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
    [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ProductDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _productService.GetPagedBySkinTypeAsync(skinTypeId, pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ProductDto>>.SuccessResponse(pagedData));
    }

    [HttpGet("by-brand/{brandId:guid}")]
    public async Task<IActionResult> GetByBrand(
    Guid brandId,
    [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
    [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ProductDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _productService.GetPagedByBrandAsync(brandId, pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ProductDto>>.SuccessResponse(pagedData));
    }


    // Add this method to the ProductController
    [HttpGet("best-sellers")]
    public async Task<IActionResult> GetBestSellers(
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ProductDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _productService.GetBestSellerAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ProductDto>>.SuccessResponse(pagedData));
    }

    // GET: api/products?pageNumber=1&pageSize=10&brandId={brandId}&categoryId={categoryId}&skinTypeId={skinTypeId}&sortBy=newest
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        [FromQuery] Guid? brandId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? skinTypeId = null,
        [FromQuery] string sortBy = "newest",
        [FromQuery] string name = null) // Thêm tham số sortBy với giá trị mặc định
    {
        // Kiểm tra tính hợp lệ của model
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<ProductDto>>.FailureResponse("Invalid query parameters", errors));
        }

        // Gọi service với các tham số lọc và sắp xếp
        var pagedData = await _productService.GetPagedAsync(pageNumber, pageSize, brandId, categoryId, skinTypeId, sortBy, name);

        // Trả về kết quả thành công
        return Ok(ApiResponse<PagedResponse<ProductDto>>.SuccessResponse(pagedData));
    }

    // POST: api/products
    [CustomAuthorize("Manager")]
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
        Guid? userId = HttpContext.Items["UserId"] as Guid?;
        try
        {
            var createdProduct = await _productService.CreateAsync(productDto, userId.ToString());
            return Ok(ApiResponse<bool>.SuccessResponse(createdProduct, "Product created successfully"));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ProductDto>.FailureResponse(ex.Message));
        }
    }

    // PATCH: api/products/{id}
    [CustomAuthorize("Manager")]
    [HttpPatch("{id:guid}")]
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
        Guid? userId = HttpContext.Items["UserId"] as Guid?;

        try
        {
            var updatedProduct = await _productService.UpdateAsync(productDto, userId.Value, id);
            return Ok(ApiResponse<bool>.SuccessResponse(updatedProduct, "Product updated successfully"));
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
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            await _productService.DeleteAsync(id, userId.ToString());
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

    // New endpoint for getting product for edit
    [HttpGet("{id:guid}/edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductForEdit(Guid id)
    {
        try
        {
            var product = await _productService.GetProductForEditAsync(id);
            return Ok(ApiResponse<ProductForEditDto>.SuccessResponse(product));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductForEditDto>.FailureResponse(ex.Message));
        }
    }

    // New endpoint for updating full product
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductForEditDto productDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<bool>.FailureResponse("Invalid product data", errors));
        }

        Guid? userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("User ID not found"));
        }

        try
        {
            var result = await _productService.UpdateProductAsync(id, productDto, userId.ToString());
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Product updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse(ex.Message));
        }
    }
}
