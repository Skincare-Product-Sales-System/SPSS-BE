using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.QuizOption;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/quiz-options")]
public class QuizOptionController : ControllerBase
{
    private readonly IQuizOptionService _quizOptionService;

    public QuizOptionController(IQuizOptionService quizOptionService)
        => _quizOptionService = quizOptionService ?? throw new ArgumentNullException(nameof(quizOptionService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var quizOption = await _quizOptionService.GetByIdAsync(id);
            return Ok(ApiResponse<QuizOptionDto>.SuccessResponse(quizOption));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizOptionDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<QuizOptionDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _quizOptionService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<QuizOptionDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] QuizOptionForCreationDto quizOptionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<QuizOptionDto>.FailureResponse("Invalid quiz option data", errors));
        }

        try
        {
            var quizOption = await _quizOptionService.CreateAsync(quizOptionDto);
            return CreatedAtAction(nameof(GetById), new { id = quizOption.Id }, ApiResponse<QuizOptionDto>.SuccessResponse(quizOption));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<QuizOptionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] QuizOptionForUpdateDto quizOptionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<QuizOptionDto>.FailureResponse("Invalid quiz option data", errors));
        }

        try
        {
            await _quizOptionService.UpdateAsync(id, quizOptionDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizOptionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-quiz-question/{quizQuestionId:guid}")]
    public async Task<IActionResult> GetByQuizQuestionId(Guid quizQuestionId)
    {
        try
        {
            var quizOptions = await _quizOptionService.GetByQuizQuestionIdAsync(quizQuestionId);
            return Ok(ApiResponse<IEnumerable<QuizOptionDto>>.SuccessResponse(quizOptions));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<QuizOptionDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("by-quiz-question/{quizQuestionId:guid}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddQuizOptionToQuizQuestion(Guid quizQuestionId, [FromBody] QuizOptionForCreationDto quizOptionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<QuizOptionDto>.FailureResponse("Invalid quiz option data", errors));
        }

        try
        {
            var quizOption = await _quizOptionService.AddQuizOptionToQuizQuestionAsync(quizQuestionId, quizOptionDto);
            return CreatedAtAction(nameof(GetById), new { id = quizOption.Id }, ApiResponse<QuizOptionDto>.SuccessResponse(quizOption));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<QuizOptionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("by-quiz-question/{quizQuestionId:guid}/{quizOptionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQuizOptionInQuizQuestion(Guid quizQuestionId, Guid quizOptionId, [FromBody] QuizOptionForUpdateDto quizOptionDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<QuizOptionDto>.FailureResponse("Invalid quiz option data", errors));
        }

        try
        {
            await _quizOptionService.UpdateQuizOptionInQuizQuestionAsync(quizQuestionId, quizOptionId, quizOptionDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizOptionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("by-quiz-question/{quizQuestionId:guid}/{quizOptionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuizOptionFromQuizQuestion(Guid quizQuestionId, Guid quizOptionId)
    {
        try
        {
            await _quizOptionService.DeleteQuizOptionFromQuizQuestionAsync(quizQuestionId, quizOptionId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizOptionDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _quizOptionService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizOptionDto>.FailureResponse(ex.Message));
        }
    }
}