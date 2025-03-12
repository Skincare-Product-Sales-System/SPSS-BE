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

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [Range(1, int.MaxValue)] int pageNumber = 1,
        [Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<CartItemDto>>.FailureResponse("Invalid pagination parameters", errors));
        }
        var pagedData = await _cartItemService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<CartItemDto>>.SuccessResponse(pagedData));
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
            var createdCartItem = await _cartItemService.CreateAsync(cartItemDto);
            var response = ApiResponse<CartItemDto>.SuccessResponse(createdCartItem, "Cart item created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdCartItem.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<CartItemDto>.FailureResponse(ex.Message));
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

        if (id != cartItemDto.Id)
            return BadRequest(ApiResponse<CartItemDto>.FailureResponse("Cart item ID in URL must match the ID in the body"));

        try
        {
            var updatedCartItem = await _cartItemService.UpdateAsync(cartItemDto);
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