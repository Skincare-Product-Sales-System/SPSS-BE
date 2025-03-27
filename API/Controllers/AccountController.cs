using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.Account;
using Services.Dto.Api;
using Services.Response;
using Microsoft.AspNetCore.Authorization;
using API.Extensions;
using Services.Implementation;

namespace API.Controllers;

[ApiController]
[Route("api/HIHIaccounts")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService) =>
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

    [CustomAuthorize("Customer")]
    // Lấy thông tin tài khoản theo UserId
    [HttpGet]
    public async Task<IActionResult> GetAccountInfo()
    {
        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
            }
            var account = await _accountService.GetAccountInfoAsync(userId.Value);
            return Ok(ApiResponse<AccountDto>.SuccessResponse(account));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AccountDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Customer")]
    // Cập nhật thông tin tài khoản
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAccountInfo([FromBody] AccountForUpdateDto accountForUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AccountForUpdateDto>.FailureResponse("Invalid account data", errors));
        }

        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
            }
            var updatedAccount = await _accountService.UpdateAccountInfoAsync(userId.Value, accountForUpdateDto);
            return Ok(ApiResponse<AccountDto>.SuccessResponse(updatedAccount, "Account updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AccountDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<AccountDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Customer")]
    [Consumes("multipart/form-data")]
    [HttpPost("upload-avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAvatar([FromForm] List<IFormFile> avatarFiles)
    {
        if (avatarFiles == null || avatarFiles.Count == 0 || avatarFiles.First().Length == 0)
        {
            return BadRequest(ApiResponse<string>.FailureResponse("Avatar file is missing or empty"));
        }

        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<string>.FailureResponse("User ID is missing or invalid"));
            }

            // Sử dụng file đầu tiên từ danh sách
            var avatarFile = avatarFiles.First();
            var avatarUrl = await _accountService.UpdateAvatarAsync(userId.Value, avatarFile);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Avatar updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("delete-avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteByImageUrl(string imageUrl)
    {
        try
        {
            var result = await _accountService.DeleteFirebaseLink(imageUrl);
            return result ? Ok(ApiResponse<object>.SuccessResponse(null, "Image deleted successfully"))
                : NotFound(ApiResponse<object>.FailureResponse("Image not found"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
