using API.Extensions;
using BusinessObjects.Dto.Account;
using BusinessObjects.Dto.SkinAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/skin-analysis")]
    public class SkinAnalysisController : ControllerBase
    {
        private readonly ISkinAnalysisService _skinAnalysisService;

        public SkinAnalysisController(ISkinAnalysisService skinAnalysisService)
        {
            _skinAnalysisService = skinAnalysisService ?? throw new ArgumentNullException(nameof(skinAnalysisService));
        }

        [CustomAuthorize("Customer")]
        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<SkinAnalysisResultDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> AnalyzeSkin(IFormFile faceImage)
        {
            try
            {
                // Validate input
                if (faceImage == null || faceImage.Length == 0)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse("Hình ?nh khuôn m?t không ???c ?? tr?ng"));
                }

                // Check file type
                var fileExtension = Path.GetExtension(faceImage.FileName).ToLower();
                if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
                {
                    return BadRequest(ApiResponse<object>.FailureResponse("Ch? ch?p nh?n các ??nh d?ng ?nh: .jpg, .jpeg, .png"));
                }

                // Check file size (limit to 10MB)
                if (faceImage.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse("Kích th??c t?p quá l?n, t?i ?a 10MB"));
                }

                // Get user ID from context (if needed)
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
                }

                // Process the image and analyze skin
                var result = await _skinAnalysisService.AnalyzeSkinAsync(faceImage);
                
                // Return success response with analysis results
                return Ok(ApiResponse<SkinAnalysisResultDto>.SuccessResponse(result, "Phân tích da thành công"));
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error analyzing skin: {ex.Message}");
                
                // Return error response
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse<object>.FailureResponse("L?i khi phân tích da", new List<string> { ex.Message }));
            }
        }
    }
}