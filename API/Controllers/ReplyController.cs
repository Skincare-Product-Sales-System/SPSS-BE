using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Dto.Reply;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/replies")]
public class ReplyController : ControllerBase
{
    private readonly IReplyService _replyService;

    public ReplyController(IReplyService replyService) =>
        _replyService = replyService ?? throw new ArgumentNullException(nameof(replyService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var reply = await _replyService.GetByIdAsync(id);
            return Ok(ApiResponse<ReplyDto>.SuccessResponse(reply));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ReplyDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<ReplyDto>>.FailureResponse("Invalid pagination parameters", errors));
        }
        var pagedData = await _replyService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<ReplyDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ReplyForCreationDto replyDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ReplyDto>.FailureResponse("Invalid reply data", errors));
        }

        try
        {
            var createdReply = await _replyService.CreateAsync(replyDto);
            var response = ApiResponse<ReplyDto>.SuccessResponse(createdReply, "Reply created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdReply.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ReplyDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ReplyForUpdateDto replyDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ReplyDto>.FailureResponse("Invalid reply data", errors));
        }

        if (id != replyDto.Id)
            return BadRequest(ApiResponse<ReplyDto>.FailureResponse("Reply ID in URL must match the ID in the body"));

        try
        {
            var updatedReply = await _replyService.UpdateAsync(replyDto);
            return Ok(ApiResponse<ReplyDto>.SuccessResponse(updatedReply, "Reply updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ReplyDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ReplyDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _replyService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Reply deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
