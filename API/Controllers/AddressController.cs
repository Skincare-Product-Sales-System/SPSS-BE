using System.ComponentModel.DataAnnotations;
using API.Extensions;
using BusinessObjects.Dto.Account;
using BusinessObjects.Dto.Address;
using BusinessObjects.Models;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

namespace API.Controllers;
[ApiController]
[Route("api/addresses")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    
    public AddressController(IAddressService addressService) => _addressService = addressService ?? throw new ArgumentNullException(nameof(addressService));

    [CustomAuthorize("Customer")]
    [HttpGet("user")]
    public async Task<IActionResult> GetByUserId(
    [Range(1, int.MaxValue)] int pageNumber = 1,
    [Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<AddressDto>>.FailureResponse("Invalid pagination parameters", errors));
        }
        Guid? userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
        }
        var pagedData = await _addressService.GetByUserIdPagedAsync(userId.Value, pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<AddressDto>>.SuccessResponse(pagedData));
    }

    [CustomAuthorize("Customer")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AddressForCreationDto addressDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AddressDto>.FailureResponse("Invalid address data", errors));
        }
        
        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
            }
            var address = await _addressService.CreateAsync(addressDto, userId.Value);
            return Ok(ApiResponse<bool>.SuccessResponse(address, "Address created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AddressDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Customer")]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AddressForUpdateDto addressDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AddressDto>.FailureResponse("Invalid address data", errors));
        }
        
        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
            }
            return Ok(ApiResponse<bool>.SuccessResponse(await _addressService.UpdateAsync(id, addressDto, userId.Value), "Address updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AddressDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Customer")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<bool>.FailureResponse("User ID is missing or invalid"));
            }
            return Ok(ApiResponse<bool>.SuccessResponse(await _addressService.DeleteAsync(id, userId.Value), "Address deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AddressDto>.FailureResponse(ex.Message)); // Trả về 404 nếu không tìm thấy Address
        }
    }

    [CustomAuthorize("Customer")]
    [HttpPatch("{id:guid}/set-default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAsDefault(Guid id)
    {
        try
        {
            // Lấy UserId từ HttpContext
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<bool>.FailureResponse("User ID is missing or invalid"));
            }

            bool isSuccess = await _addressService.SetAsDefaultAsync(id, userId.Value);
            return Ok(ApiResponse<bool>.SuccessResponse(isSuccess, "Address set as default successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse($"An error occurred: {ex.Message}"));
        }
    }
}