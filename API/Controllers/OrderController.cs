using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.Order;
using Services.Dto.Api;
using Services.Response;
using API.Extensions;

namespace API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService) => _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

        [CustomAuthorize("Manager")]
        [HttpGet("canceled-orders")]
        public async Task<IActionResult> GetCanceledOrders()
        {
            try
            {
                // Retrieve all canceled orders
                var canceledOrders = await _orderService.GetCanceledOrdersAsync();
                return Ok(ApiResponse<List<CanceledOrderDto>>.SuccessResponse(canceledOrders));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<CanceledOrderDto>>.FailureResponse(
                    "Failed to retrieve canceled orders",
                    new List<string> { ex.Message }
                ));
            }
        }

        [CustomAuthorize("Customer")]
        [HttpGet("user")]
        public async Task<IActionResult> GetOrdersByUserId(
        [Range(1, int.MaxValue)] int pageNumber = 1,
        [Range(1, 100)] int pageSize = 10,
        string? status = null)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PagedResponse<OrderDto>>.FailureResponse("Invalid pagination parameters", errors));
            }

            try
            {
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<PagedResponse<OrderDto>>.FailureResponse("Unauthorized access", new List<string> { "User ID not found in context." }));
                }

                // Retrieve orders with the optional status filter
                var pagedData = await _orderService.GetOrdersByUserIdAsync(userId.Value, pageNumber, pageSize, status);
                return Ok(ApiResponse<PagedResponse<OrderDto>>.SuccessResponse(pagedData));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PagedResponse<OrderDto>>.FailureResponse("Failed to retrieve orders", new List<string> { ex.Message }));
            }
        }

        [HttpGet("total-orders")]
        public async Task<IActionResult> GetTotalOrdersByUserId()
        {
            try
            {
                // Lấy UserId từ context
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<int>.FailureResponse("Unauthorized access", new List<string> { "User ID not found in context." }));
                }

                // Gọi service để đếm tổng số đơn hàng của người dùng
                var totalOrders = await _orderService.GetTotalOrdersByUserIdAsync(userId.Value);
                return Ok(ApiResponse<int>.SuccessResponse(totalOrders));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<int>.FailureResponse("Failed to retrieve total orders", new List<string> { ex.Message }));
            }
        }

        [CustomAuthorize("Manager", "Customer")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                return Ok(ApiResponse<OrderWithDetailDto>.SuccessResponse(order));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderDto>.FailureResponse(ex.Message));
            }
        }

        [CustomAuthorize("Manager")]
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [Range(1, int.MaxValue)] int pageNumber = 1,
            [Range(1, 500)] int pageSize = 10)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PagedResponse<OrderDto>>.FailureResponse("Invalid pagination parameters", errors));
            }

            var pagedData = await _orderService.GetPagedAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResponse<OrderDto>>.SuccessResponse(pagedData));
        }

        [CustomAuthorize("Customer")]
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

            Guid? userId = HttpContext.Items["UserId"] as Guid?;
            try
            {
                var createdOrder = await _orderService.CreateAsync(orderDto, userId.Value);
                var response = ApiResponse<OrderDto>.SuccessResponse(createdOrder, "Order created successfully");
                return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, response);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ApiResponse<OrderDto>.FailureResponse(ex.Message));
            }
        }

        //[CustomAuthorize("Manager", "Customer")]
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, string newStatus = "Cancelled", Guid? cancelReasonId = null)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OrderDto>.FailureResponse("Invalid status data", errors));
            }

            // Truyền Guid.Empty thay vì userId.Value để tránh lỗi nullable
            try
            {
                var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, newStatus, Guid.Empty, cancelReasonId);
                return Ok(ApiResponse<bool>.SuccessResponse(updatedOrder, "Order status updated successfully"));
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

        [CustomAuthorize("Customer")]
        [HttpPatch("{id:guid}/payment-method")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderPaymentMethod(Guid id, Guid paymentMethodId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.FailureResponse("Invalid payment method data", errors));
            }

            Guid? userId = HttpContext.Items["UserId"] as Guid?;

            try
            {
                var updated = await _orderService.UpdateOrderPaymentMethodAsync(id, paymentMethodId, userId.Value);
                return Ok(ApiResponse<bool>.SuccessResponse(updated, "Payment method updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.FailureResponse(ex.Message));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ApiResponse<bool>.FailureResponse(ex.Message));
            }
        }

        [CustomAuthorize("Customer")]
        [HttpPatch("{id:guid}/address")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderAddress(Guid id, Guid newAddressId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OrderDto>.FailureResponse("Invalid address data", errors));
            }

            Guid? userId = HttpContext.Items["UserId"] as Guid?;

            try
            {
                var updatedOrder = await _orderService.UpdateOrderAddressAsync(id, newAddressId, userId.Value);
                return Ok(ApiResponse<bool>.SuccessResponse(updatedOrder, "Order address updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderDto>.FailureResponse(ex.Message));
            }
        }
        
        
        //[HttpDelete("{id:guid}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> Delete(Guid id)
        //{
        //    try
        //    {
        //        Guid? userId = HttpContext.Items["UserId"] as Guid?;
        //        await _orderService.DeleteAsync(id, userId);
        //        return Ok(ApiResponse<object>.SuccessResponse(null, "Order deleted successfully"));
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        //    }
        //}
    }
}
