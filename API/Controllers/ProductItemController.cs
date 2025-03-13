using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Implementation;
using Services.Interface;

namespace API.Controllers
{
    [ApiController]
    [Route("api/product-images")]
    public class ProductItemController : ControllerBase
    {
        private readonly IProductItemService _productItemService;

        public ProductItemController(IProductItemService productItemService)
        {
            _productItemService = productItemService ?? throw new ArgumentNullException(nameof(productItemService));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImage(Guid id, [FromForm] List<IFormFile> files)
        {
            if (!ModelState.IsValid || files == null || files.Count == 0)
            {
                return BadRequest(ApiResponse<bool>.FailureResponse("No files uploaded"));
            }

            try
            {
                var result = await _productItemService.UploadProductItemImage(files, id);
                return result ? Ok(ApiResponse<bool>.SuccessResponse(true, "Images uploaded successfully"))
                              : BadRequest(ApiResponse<bool>.FailureResponse("Failed to upload images"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailureResponse(ex.Message));
            }
        }
    }
}
