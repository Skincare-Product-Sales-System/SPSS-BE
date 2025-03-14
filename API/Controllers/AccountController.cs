using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.Account;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService) =>
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

    // Lấy thông tin tài khoản theo UserId
    [HttpGet]
    public async Task<IActionResult> GetAccountInfo()
    {
        try
        {
            //Guid? userId = HttpContext.Items["UserId"] as Guid?;
            //if (userId == null)
            //{
            //    return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
            //}
            //var account = await _accountService.GetAccountInfoAsync(userId.Value);
            Guid userId = Guid.Parse("032b11dc-c5bb-42ec-a319-9b691339ecc0"); //hard code for testing
            var account = await _accountService.GetAccountInfoAsync(userId);
            return Ok(ApiResponse<AccountDto>.SuccessResponse(account));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AccountDto>.FailureResponse(ex.Message));
        }
    }

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
}
