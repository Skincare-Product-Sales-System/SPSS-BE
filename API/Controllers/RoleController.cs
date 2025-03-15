using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.Role;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/roles")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService) => _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(id);
            return Ok(ApiResponse<RoleDto>.SuccessResponse(role));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RoleDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([Range(1, int.MaxValue)] int pageNumber = 1, [Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<RoleDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _roleService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<RoleDto>>.SuccessResponse(pagedData));
    }

    [CustomAuthorize("Manager")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] RoleForCreationDto roleDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<RoleDto>.FailureResponse("Invalid role data", errors));
        }

        try
        {
            var createdRole = await _roleService.CreateAsync(roleDto);
            return CreatedAtAction(nameof(GetById), new { id = createdRole.RoleId }, ApiResponse<RoleDto>.SuccessResponse(createdRole));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Manager")]
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoleForUpdateDto roleDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<RoleDto>.FailureResponse("Invalid role data", errors));
        }

        try
        {
            await _roleService.UpdateAsync(id, roleDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RoleDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Manager")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _roleService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RoleDto>.FailureResponse(ex.Message));
        }
    }
}
