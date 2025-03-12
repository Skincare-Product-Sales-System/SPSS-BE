using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Dto.CartItem;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/cart-items")]
public class CartItemController : ControllerBase
{
    private readonly ICartItemService _cartItemService;

    public CartItemController(ICartItemService cartItemService) =>
        _cartItemService = cartItemService ?? throw new ArgumentNullException(nameof(cartItemService));

    [HttpGet("user/{userId:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var cartItems = await _cartItemService.GetByUserIdAsync(userId);
        if (cartItems == null || !cartItems.Any())
            return NotFound(ApiResponse<IEnumerable<CartItemDto>>.FailureResponse("No cart items found for the specified user."));

        return Ok(ApiResponse<IEnumerable<CartItemDto>>.SuccessResponse(cartItems, "Cart items retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var cartItem = await _cartItemService.GetByIdAsync(id);
            return Ok(ApiResponse<CartItemDto>.SuccessResponse(cartItem));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CartItemDto>.FailureResponse(ex.Message));
        }
    }

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
            Guid userId = Guid.Parse("032b11dc-c5bb-42ec-a319-9b691339ecc0"); // Hardcoded for now
            var createdCartItem = await _cartItemService.CreateAsync(cartItemDto, userId);
            return  Ok(ApiResponse<bool>.SuccessResponse(createdCartItem, "Cart item created successfully"));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
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