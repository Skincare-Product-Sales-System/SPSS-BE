using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.PaymentMethod;
using Services.Dto.Api;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/payment-methods")]
public class PaymentMethodController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentMethodController(IPaymentMethodService paymentMethodService) =>
        _paymentMethodService = paymentMethodService ?? throw new ArgumentNullException(nameof(paymentMethodService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var paymentMethod = await _paymentMethodService.GetByIdAsync(id);
            return Ok(ApiResponse<PaymentMethodDto>.SuccessResponse(paymentMethod));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentMethodDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<PaymentMethodDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _paymentMethodService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<PaymentMethodDto>>.SuccessResponse(pagedData));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PaymentMethodForCreationDto paymentMethodDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PaymentMethodDto>.FailureResponse("Invalid payment method data", errors));
        }
        string userId = "System";
        try
        {
            var createdPaymentMethod = await _paymentMethodService.CreateAsync(paymentMethodDto, userId);
            var response = ApiResponse<PaymentMethodDto>.SuccessResponse(createdPaymentMethod, "Payment method created successfully");
            return CreatedAtAction(nameof(GetById), new { id = createdPaymentMethod.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<PaymentMethodDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PaymentMethodForUpdateDto paymentMethodDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PaymentMethodDto>.FailureResponse("Invalid payment method data", errors));
        }
        string userId = "System";
        try
        {
            var updatedPaymentMethod = await _paymentMethodService.UpdateAsync(id, paymentMethodDto, userId);
            return Ok(ApiResponse<PaymentMethodDto>.SuccessResponse(updatedPaymentMethod, "Payment method updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentMethodDto>.FailureResponse(ex.Message));
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ApiResponse<PaymentMethodDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            string userId = "System";
            await _paymentMethodService.DeleteAsync(id, userId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Payment method deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
