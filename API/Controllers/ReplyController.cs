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
            Guid? userId = HttpContext.Items["UserId"] as Guid?; //hard code for testing
            var createdReply = await _replyService.CreateAsync(userId.Value, replyDto);
            return Ok(ApiResponse<ReplyDto>.SuccessResponse(createdReply, "Reply created successfully"));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<ReplyDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
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

        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?; //hard code for testing
            var updatedReply = await _replyService.UpdateAsync(userId.Value, replyDto, id);
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
            Guid? userId = HttpContext.Items["UserId"] as Guid?; //hard code for testing
            await _replyService.DeleteAsync(userId.Value, id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Reply deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
