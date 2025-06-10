using API.Extensions;
using BusinessObjects.Dto.Account;
using BusinessObjects.Dto.SkinAnalysis;
using BusinessObjects.Dto.Transaction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace API.Controllers
{
    [ApiController]
    [Route("api/skin-analysis")]
    public class SkinAnalysisController : ControllerBase
    {
        private readonly ISkinAnalysisService _skinAnalysisService;
        private readonly ITransactionService _transactionService;
        private readonly IHubContext<TransactionHub> _transactionHubContext;

        public SkinAnalysisController(
            ISkinAnalysisService skinAnalysisService,
            ITransactionService transactionService,
            IHubContext<TransactionHub> transactionHubContext)
        {
            _skinAnalysisService = skinAnalysisService ?? throw new ArgumentNullException(nameof(skinAnalysisService));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _transactionHubContext = transactionHubContext ?? throw new ArgumentNullException(nameof(transactionHubContext));
        }

        [CustomAuthorize("Customer")]
        [HttpPost("create-payment")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> CreatePaymentRequest()
        {
            try
            {
                // Get user ID from context
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
                }

                // Create payment transaction
                var transaction = await _skinAnalysisService.CreateSkinAnalysisPaymentRequestAsync(userId.Value);
                
                // G?i thông báo v? giao d?ch m?i t?i admin qua SignalR
                await _transactionHubContext.Clients.All.SendAsync("NewTransaction", transaction);
                
                return Ok(ApiResponse<TransactionDto>.SuccessResponse(transaction, 
                    "Yêu cầu thanh toán đã được tạo. Vui lòng chuyểnn khoản theo thông tin được cung cấp."));
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating payment request: {ex.Message}");
                
                // Return error response
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse<object>.FailureResponse("Lỗi khi tạo yêu c?u thanh toán", new List<string> { ex.Message }));
            }
        }
        
        [CustomAuthorize("Customer")]
        [HttpPost("analyze-with-payment")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<SkinAnalysisResultDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> AnalyzeSkinWithPayment(IFormFile faceImage)
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

                // Get user ID from context
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
                }

                // Check payment status and proceed with analysis if approved
                var response = await _skinAnalysisService.CheckPaymentStatusAndAnalyzeSkinAsync(faceImage, userId.Value);
                
                return Ok(response);
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

        [CustomAuthorize("Manager")]
        [HttpPost("approve-and-analyze")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> ApproveAndAnalyze([FromBody] UpdateTransactionStatusDto dto)
        {
            try
            {
                // Validate input
                if (dto == null)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse("Thông tin giao d?ch không ???c ?? tr?ng"));
                }

                // Get admin ID from context
                Guid? adminId = HttpContext.Items["UserId"] as Guid?;
                if (adminId == null)
                {
                    return BadRequest(ApiResponse<AccountDto>.FailureResponse("Admin ID is missing or invalid"));
                }

                // Update transaction status
                var transaction = await _transactionService.UpdateTransactionStatusAsync(dto, adminId.Value.ToString());
                
                // G?i thông báo v? vi?c giao d?ch ?ã ???c duy?t qua SignalR
                await _transactionHubContext.Clients.All.SendAsync("TransactionUpdated", transaction);
                
                return Ok(ApiResponse<TransactionDto>.SuccessResponse(transaction, 
                    $"Giao d?ch ?ã ???c c?p nh?t thành {dto.Status}"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("L?i khi duy?t giao d?ch", new List<string> { ex.Message }));
            }
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

                // Get user ID from context
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
                }

                // Process the image and analyze skin - now passing the user ID
                var result = await _skinAnalysisService.AnalyzeSkinAsync(faceImage, userId.Value);
                
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

        [CustomAuthorize("Customer")]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<SkinAnalysisResultDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetSkinAnalysisById(Guid id)
        {
            try
            {
                var result = await _skinAnalysisService.GetSkinAnalysisResultByIdAsync(id);
                return Ok(ApiResponse<SkinAnalysisResultDto>.SuccessResponse(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("L?i khi l?y k?t qu? phân tích da", new List<string> { ex.Message }));
            }
        }

        [CustomAuthorize("Customer")]
        [HttpGet("user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<SkinAnalysisResultDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetSkinAnalysisByUserId()
        {
            try
            {
                // Get user ID from context
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
                }

                var results = await _skinAnalysisService.GetSkinAnalysisResultsByUserIdAsync(userId.Value);
                return Ok(ApiResponse<List<SkinAnalysisResultDto>>.SuccessResponse(results));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("L?i khi l?y l?ch s? phân tích da", new List<string> { ex.Message }));
            }
        }

        [CustomAuthorize("Customer")]
        [HttpGet("user/paged")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PagedResponse<SkinAnalysisResultDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetPagedSkinAnalysisByUserId(
            [Range(1, int.MaxValue)] int pageNumber = 1,
            [Range(1, 100)] int pageSize = 10)
        {
            try
            {
                // Get user ID from context
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
                }

                var results = await _skinAnalysisService.GetSkinAnalysisResultsByUserIdAsync(userId.Value);
                
                // Manual paging (ideally this would be in the service layer with proper DB queries)
                var totalCount = results.Count;
                var skip = (pageNumber - 1) * pageSize;
                var pagedItems = results
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                var pagedResponse = new PagedResponse<SkinAnalysisResultDto>
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(ApiResponse<PagedResponse<SkinAnalysisResultDto>>.SuccessResponse(pagedResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("L?i khi l?y l?ch s? phân tích da", new List<string> { ex.Message }));
            }
        }

        [CustomAuthorize("Manager")]
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PagedResponse<SkinAnalysisResultDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetAllSkinAnalysisResults(
        [Range(1, int.MaxValue)] int pageNumber = 1,
        [Range(1, 100)] int pageSize = 10,
        [FromQuery] string skinType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
        {
            try
            {
                // Get admin ID from context for audit/logging purposes
                Guid? adminId = HttpContext.Items["UserId"] as Guid?;
                if (adminId == null)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse("Admin ID is missing or invalid"));
                }

                // Call service to get paged skin analysis results with optional filters
                var pagedResults = await _skinAnalysisService.GetAllSkinAnalysisResultsAsync(
                    pageNumber,
                    pageSize,
                    skinType,
                    fromDate,
                    toDate);

                return Ok(ApiResponse<PagedResponse<SkinAnalysisResultDto>>.SuccessResponse(
                    pagedResults,
                    "Danh sách kết quả phân tích da được trả về thành công"));
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving all skin analysis results: {ex.Message}");

                // Return error response
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Lỗi khi lấy danh sách kết quả phân tích da",
                        new List<string> { ex.Message }));
            }
        }
    }
}