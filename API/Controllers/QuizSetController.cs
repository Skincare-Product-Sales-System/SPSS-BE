using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
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
    }
}
