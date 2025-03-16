using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.QuizQuestion;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/quiz-questions")]
public class QuizQuestionController : ControllerBase
{
    private readonly IQuizQuestionService _quizQuestionService;

    public QuizQuestionController(IQuizQuestionService quizQuestionService)
        => _quizQuestionService = quizQuestionService ?? throw new ArgumentNullException(nameof(quizQuestionService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var quizQuestion = await _quizQuestionService.GetByIdAsync(id);
            return Ok(ApiResponse<QuizQuestionDto>.SuccessResponse(quizQuestion));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizQuestionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [Range(1, int.MaxValue)] int pageNumber = 1,
        [Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<QuizQuestionDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _quizQuestionService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<QuizQuestionDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] QuizQuestionForCreationDto quizQuestionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<QuizQuestionDto>.FailureResponse("Invalid quiz question data", errors));
        }

        try
        {
            var quizQuestion = await _quizQuestionService.CreateAsync(quizQuestionDto);
            return CreatedAtAction(nameof(GetById), new { id = quizQuestion.Id }, ApiResponse<QuizQuestionDto>.SuccessResponse(quizQuestion));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<QuizQuestionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] QuizQuestionForUpdateDto quizQuestionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<QuizQuestionDto>.FailureResponse("Invalid quiz question data", errors));
        }

        try
        {
            await _quizQuestionService.UpdateAsync(id, quizQuestionDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizQuestionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-quiz-set/{quizSetId:guid}")]
    public async Task<IActionResult> GetByQuizSetId(Guid quizSetId)
    {
        try
        {
            var quizQuestions = await _quizQuestionService.GetByQuizSetIdAsync(quizSetId);
            return Ok(ApiResponse<IEnumerable<QuizQuestionDto>>.SuccessResponse(quizQuestions));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<QuizQuestionDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("by-quiz-set/{quizSetId:guid}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddQuizQuestionToQuizSet(Guid quizSetId, [FromBody] QuizQuestionForCreationDto quizQuestionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<QuizQuestionDto>.FailureResponse("Invalid quiz question data", errors));
        }

        try
        {
            var quizQuestion = await _quizQuestionService.AddQuizQuestionToQuizSetAsync(quizSetId, quizQuestionDto);
            return CreatedAtAction(nameof(GetById), new { id = quizQuestion.Id }, ApiResponse<QuizQuestionDto>.SuccessResponse(quizQuestion));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<QuizQuestionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("by-quiz-set/{quizSetId:guid}/{quizQuestionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQuizQuestionInQuizSet(Guid quizSetId, Guid quizQuestionId, [FromBody] QuizQuestionForUpdateDto quizQuestionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<QuizQuestionDto>.FailureResponse("Invalid quiz question data", errors));
        }

        try
        {
            await _quizQuestionService.UpdateQuizQuestionInQuizSetAsync(quizSetId, quizQuestionId, quizQuestionDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizQuestionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("by-quiz-set/{quizSetId:guid}/{quizQuestionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuizQuestionFromQuizSet(Guid quizSetId, Guid quizQuestionId)
    {
        try
        {
            await _quizQuestionService.DeleteQuizQuestionFromQuizSetAsync(quizSetId, quizQuestionId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizQuestionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _quizQuestionService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizQuestionDto>.FailureResponse(ex.Message));
        }
    }
}