using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.Order;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService) => _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                return Ok(ApiResponse<OrderDto>.SuccessResponse(order));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderDto>.FailureResponse(ex.Message));
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
                return BadRequest(ApiResponse<PagedResponse<OrderDto>>.FailureResponse("Invalid pagination parameters", errors));
            }

            var pagedData = await _orderService.GetPagedAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResponse<OrderDto>>.SuccessResponse(pagedData));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] OrderForCreationDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OrderDto>.FailureResponse("Invalid order data", errors));
            }

            Guid userId = Guid.Parse("12e6ef03-e72c-407d-894e-fd3d17f66756"); // Hardcoded for demo purposes
            try
            {
                var createdOrder = await _orderService.CreateAsync(orderDto, userId);
                var response = ApiResponse<OrderDto>.SuccessResponse(createdOrder, "Order created successfully");
                return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, response);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ApiResponse<OrderDto>.FailureResponse(ex.Message));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderForUpdateDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OrderDto>.FailureResponse("Invalid order data", errors));
            }

            Guid userId = Guid.Parse("12e6ef03-e72c-407d-894e-fd3d17f66756"); // Hardcoded for demo purposes
            try
            {
                var updatedOrder = await _orderService.UpdateAsync(id, orderDto, userId);
                return Ok(ApiResponse<OrderDto>.SuccessResponse(updatedOrder, "Order updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderDto>.FailureResponse(ex.Message));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ApiResponse<OrderDto>.FailureResponse(ex.Message));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                Guid userId = Guid.Parse("12e6ef03-e72c-407d-894e-fd3d17f66756"); // Hardcoded for demo purposes
                await _orderService.DeleteAsync(id, userId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Order deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            }
        }
    }
}
