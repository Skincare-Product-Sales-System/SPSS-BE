using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;

namespace API.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageController : Controller
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("migrateToFirebaseLinks")]
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
                var result = await _imageService.MigrateToFirebaseLinkList(files);
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


        [HttpDelete("DeleteByImageUrl")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteByImageUrl(string imageUrl)
        {
            try
            {
                var result = await _imageService.DeleteFirebaseLink(imageUrl);
                return result ? Ok(ApiResponse<object>.SuccessResponse(null, "Image deleted successfully"))
                    : NotFound(ApiResponse<object>.FailureResponse("Image not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
            }
        }
    }
}
