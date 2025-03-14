using BusinessObjects.Dto.QuizSet;
using BusinessObjects.Dto.SkinType;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Implementation;
using Services.Interface;
using Services.Response;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/quiz-sets")]
    [ApiController]
    public class QuizSetController : ControllerBase
    {
        private readonly IQuizSetService _quizSetService;

        public QuizSetController(IQuizSetService quizSetService)
        {
            _quizSetService = quizSetService;
        }
        [HttpGet("{quizSetId}/questions")]
        public async Task<IActionResult> GetQuizSetQuestions(Guid quizSetId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (quizSetId == Guid.Empty)
            {
                return BadRequest(new { message = "QuizSetId không hợp lệ" });
            }

            var result = await _quizSetService.GetQuizSetWithQuestionsAsync(quizSetId, pageNumber, pageSize);
            return Ok(new { data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged(
        [Range(1, int.MaxValue)] int pageNumber = 1,
        [Range(1, 100)] int pageSize = 10)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PagedResponse<QuizSetDto>>.FailureResponse("Invalid pagination parameters", errors));
            }

            var pagedData = await _quizSetService.GetPagedAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResponse<QuizSetDto>>.SuccessResponse(pagedData));
        }
    }
}
