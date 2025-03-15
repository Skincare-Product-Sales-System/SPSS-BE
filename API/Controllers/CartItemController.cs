using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Dto.CartItem;
using Services.Dto.Api;
using Services.Response;
using Microsoft.AspNetCore.Authorization;
using API.Extensions;
using BusinessObjects.Dto.Account;

namespace API.Controllers;

[ApiController]
[Route("api/cart-items")]
public class CartItemController : ControllerBase
{
    private readonly ICartItemService _cartItemService;

    public CartItemController(ICartItemService cartItemService) =>
        _cartItemService = cartItemService ?? throw new ArgumentNullException(nameof(cartItemService));

    [CustomAuthorize("Customer")]
    [HttpGet("user/cart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart(
    [Range(1, int.MaxValue)] int pageNumber = 1,
    [Range(1, 100)] int pageSize = 10)
    {
        Guid? userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
        }

        // Gọi service để lấy cart items.
        var cartItems = await _cartItemService.GetByUserIdAsync(userId.Value, pageNumber, pageSize);

        // Xử lý nếu không có cart items.
        if (cartItems == null || !cartItems.Items.Any())
            return NotFound(ApiResponse<PagedResponse<CartItemDto>>.FailureResponse("No cart items found for the specified user."));

        // Trả về kết quả thành công.
        return Ok(ApiResponse<PagedResponse<CartItemDto>>.SuccessResponse(cartItems, "Cart items retrieved successfully"));
    }

    [CustomAuthorize("Customer")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CartItemForCreationDto cartItemDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CartItemDto>.FailureResponse("Invalid cart item data", errors));
        }

        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            if (userId == null)
            {
                return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
            }
            var createdCartItem = await _cartItemService.CreateAsync(cartItemDto, userId.Value);
            return  Ok(ApiResponse<bool>.SuccessResponse(createdCartItem, "Cart item created successfully"));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Customer")]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CartItemForUpdateDto cartItemDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CartItemDto>.FailureResponse("Invalid cart item data", errors));
        }

        try
        {
            var updatedCartItem = await _cartItemService.UpdateAsync(id, cartItemDto);
            return Ok(ApiResponse<CartItemDto>.SuccessResponse(updatedCartItem, "Cart item updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CartItemDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<CartItemDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Customer")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _cartItemService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Cart item deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}