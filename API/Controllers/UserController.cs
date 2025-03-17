using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BusinessObjects.Dto.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

  
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        return userId;
    }

    
    [HttpGet]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPaged(
        [Range(1, int.MaxValue)] int pageNumber = 1,
        [Range(1, 100)] int pageSize = 10)
    {
        var pagedUsers = await _userService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<UserDto>>.SuccessResponse(pagedUsers));
    }


    [HttpGet("{id:guid}")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] UserForCreationDto userDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<UserDto>.FailureResponse("Invalid user data", errors));
        }

        try
        {
            var createdUser = await _userService.CreateAsync(userDto);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.UserId }, ApiResponse<UserDto>.SuccessResponse(createdUser));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserDto>.FailureResponse(ex.Message));
        }
    }


    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserForUpdateDto userDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<UserDto>.FailureResponse("Invalid user data", errors));
        }

        var currentUserId = GetUserId();
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && id != currentUserId)
            return Forbid("You can only update your own profile.");

        try
        {
            await _userService.UpdateAsync(id, userDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.FailureResponse(ex.Message));
        }
    }

  
    [HttpDelete("{id:guid}")]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.FailureResponse(ex.Message));
        }
    }
}
