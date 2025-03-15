using System.ComponentModel.DataAnnotations;
using API.Extensions;
using BusinessObjects.Dto.Voucher;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

namespace API.Controllers;

[ApiController]
[Route("api/voucher")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    public VoucherController(IVoucherService voucherService) 
        => _voucherService = voucherService ?? throw new ArgumentNullException(nameof(voucherService));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var voucher = await _voucherService.GetByIdAsync(id);
            return Ok(ApiResponse<VoucherDto>.SuccessResponse(voucher));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VoucherDto>.FailureResponse(ex.Message));
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
            return BadRequest(ApiResponse<PagedResponse<VoucherDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedData = await _voucherService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<VoucherDto>>.SuccessResponse(pagedData));
    }

    [CustomAuthorize("Manager")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] VoucherForCreationDto voucherDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<VoucherDto>.FailureResponse("Invalid voucher data", errors));
        }

        try
        {
            var voucher = await _voucherService.CreateAsync(voucherDto);
            return CreatedAtAction(nameof(GetById), new { id = voucher.Id }, ApiResponse<VoucherDto>.SuccessResponse(voucher));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<VoucherDto>.FailureResponse(ex.Message));
        }
    }

    [CustomAuthorize("Manager")]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] VoucherForUpdateDto voucherDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<VoucherDto>.FailureResponse("Invalid voucher data", errors));
        }

        try
        {
            await _voucherService.UpdateAsync(id, voucherDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VoucherDto>.FailureResponse(ex.Message));
        }
    }
}
