using BusinessObjects.Dto.QuizSet;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] QuizSetForCreationDto quizSetDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<QuizSetDto>.FailureResponse("Invalid quiz set data", errors));
            }

            try
            {
                var quizSet = await _quizSetService.CreateAsync(quizSetDto);
                return CreatedAtAction(nameof(GetQuizSetQuestions), new { quizSetId = quizSet.Id }, ApiResponse<QuizSetDto>.SuccessResponse(quizSet));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<QuizSetDto>.FailureResponse(ex.Message));
            }
        }

        [HttpPatch("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] QuizSetForUpdateDto quizSetDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<QuizSetDto>.FailureResponse("Invalid quiz set data", errors));
            }

            try
            {
                await _quizSetService.UpdateAsync(id, quizSetDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuizSetDto>.FailureResponse(ex.Message));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _quizSetService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuizSetDto>.FailureResponse(ex.Message));
            }
        }
        
        
        [HttpPatch("set-default/{quizSetId:guid}")]
        public async Task<IActionResult> SetQuizSetDefault(Guid quizSetId)
        {
            try
            {
                await _quizSetService.SetQuizSetDefaultAsync(quizSetId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuizSetDto>.FailureResponse(ex.Message));
            }
        }
    }
}