using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.CancelReason;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/cancel-reasons")]
public class CancelReasonController : ControllerBase
{
    private readonly ICancelReasonService _cancelReasonService;

    public CancelReasonController(ICancelReasonService cancelReasonService) => _cancelReasonService = cancelReasonService ?? throw new ArgumentNullException(nameof(cancelReasonService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var cancelReason = await _cancelReasonService.GetByIdAsync(id);
            return Ok(ApiResponse<CancelReasonDto>.SuccessResponse(cancelReason));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CancelReasonDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<CancelReasonDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _cancelReasonService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<CancelReasonDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CancelReasonForCreationDto cancelReasonDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CancelReasonDto>.FailureResponse("Invalid cancel reason data", errors));
        }
        Guid userId = Guid.Parse("12e6ef03-e72c-407d-894e-fd3d17f66756"); //Hardcoded for demo purposes
        try
        {
            var createdCancelReason = await _cancelReasonService.CreateAsync(cancelReasonDto, userId);
            var response = ApiResponse<CancelReasonDto>.SuccessResponse(createdCancelReason, "Cancel reason created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdCancelReason.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<CancelReasonDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CancelReasonForUpdateDto cancelReasonDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CancelReasonDto>.FailureResponse("Invalid cancel reason data", errors));
        }
        Guid userId = Guid.Parse("12e6ef03-e72c-407d-894e-fd3d17f66756"); //Hardcoded for demo purposes
        try
        {
            var updatedCancelReason = await _cancelReasonService.UpdateAsync(id, cancelReasonDto, userId);
            return Ok(ApiResponse<CancelReasonDto>.SuccessResponse(updatedCancelReason, "Cancel reason updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CancelReasonDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<CancelReasonDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            Guid userId = Guid.Parse("12e6ef03-e72c-407d-894e-fd3d17f66756"); //Hardcoded for demo purposes
            await _cancelReasonService.DeleteAsync(id, userId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Cancel reason deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}