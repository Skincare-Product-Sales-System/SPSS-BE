using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Services.Dto.Api;

namespace API.Controllers
{
    [ApiController]
    [Route("api/product-images")]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        public ProductImageController(IProductImageService productImageService)
        {
            _productImageService = productImageService ?? throw new ArgumentNullException(nameof(productImageService));
        }

        // Upload product images
        [HttpPost("{productId:guid}/upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(Guid productId, [FromForm] List<IFormFile> files)
        {
            if (!ModelState.IsValid || files == null || files.Count == 0)
            {
                return BadRequest(ApiResponse<bool>.FailureResponse("No files uploaded"));
            }

            try
            {
                var result = await _productImageService.UploadProductImage(files, productId);
                return result ? Ok(ApiResponse<bool>.SuccessResponse(true, "Images uploaded successfully")) 
                              : BadRequest(ApiResponse<bool>.FailureResponse("Failed to upload images"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailureResponse(ex.Message));
            }
        }

        // Delete product image
        [HttpDelete("{imageId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid imageId)
        {
            try
            {
                var result = await _productImageService.DeleteProductImage(imageId);
                return result ? Ok(ApiResponse<object>.SuccessResponse(null, "Image deleted successfully"))
                              : NotFound(ApiResponse<object>.FailureResponse("Image not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
            }
        }

        // Get product images by product id
        [HttpGet("{productId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            try
            {
                var images = await _productImageService.GetProductImageById(productId);
                return images != null && images.Any() 
                    ? Ok(ApiResponse<IList<ProductImageByIdResponse>>.SuccessResponse(images)) 
                    : NotFound(ApiResponse<IList<ProductImageByIdResponse>>.FailureResponse("No images found for this product"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IList<ProductImageByIdResponse>>.FailureResponse(ex.Message));
            }
        }
        
        [HttpPost("/migrateToFirebaseLinks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MigrateToFirebaseLink([FromForm] List<IFormFile> files)
        {
            if (!ModelState.IsValid || files == null || files.Count == 0)
            {
                return BadRequest(ApiResponse<bool>.FailureResponse("No files uploaded"));
            }

            try
            {
                var result = await _productImageService.MigrateToFirebaseLinkList(files);
                if (result != null && result.Any())
                {
                    return Ok(ApiResponse<IList<string>>.SuccessResponse(result, "Images uploaded successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<bool>.FailureResponse("Failed to upload images"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailureResponse(ex.Message));
            }
        }
    }
}
