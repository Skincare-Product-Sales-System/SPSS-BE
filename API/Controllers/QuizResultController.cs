using BusinessObjects.Dto.Account;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/quiz-results")]
    [ApiController]
    public class QuizResultController : ControllerBase
    {
        private readonly IQuizResultService _quizResultService;

        public QuizResultController(IQuizResultService quizResultService)
        {
            _quizResultService = quizResultService;
        }

        [HttpGet("by-point-and-set")]
        public async Task<IActionResult> GetByPointAndSetIdAsync([FromQuery] string score, [FromQuery] Guid quizSetId)
        {
            if (string.IsNullOrEmpty(score) || quizSetId == Guid.Empty)
            {
                return BadRequest(new { message = "Score hoặc QuizSetId không hợp lệ" });
            }

            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
            }
            var result = await _quizResultService.GetByPointAndSetIdAsync(score, quizSetId, userId.Value);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy kết quả quiz" });
            }

            return Ok(new { data = result });
        }
    }
}
