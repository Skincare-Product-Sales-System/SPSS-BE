using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.User;
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

    public UserController(IUserService userService) => _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged([Range(1, int.MaxValue)] int pageNumber = 1, [Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<UserDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _userService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<UserDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
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
            var user = await _userService.CreateAsync(userDto);
            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, ApiResponse<UserDto>.SuccessResponse(user));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UserForUpdateDto userDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<UserDto>.FailureResponse("Invalid user data", errors));
        }

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

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
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
