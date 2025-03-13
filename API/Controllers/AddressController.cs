using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dto.Address;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;

namespace API.Controllers;
[ApiController]
[Route("api/address")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    
    public AddressController(IAddressService addressService) => _addressService = addressService ?? throw new ArgumentNullException(nameof(addressService));
   
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var address = await _addressService.GetByIdAsync(id);
            return Ok(ApiResponse<AddressDto>.SuccessResponse(address));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AddressDto>.FailureResponse(ex.Message));
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetPaged([Range(1, int.MaxValue)] int pageNumber = 1, [Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<AddressDto>>.FailureResponse("Invalid pagination parameters", errors));
        }
        
        var pagedData = await _addressService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<AddressDto>>.SuccessResponse(pagedData));
    }
    
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
            var address = await _addressService.CreateAsync(addressDto);
            return CreatedAtAction(nameof(GetById), new { id = address.Id }, ApiResponse<AddressDto>.SuccessResponse(address));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AddressDto>.FailureResponse(ex.Message));
        }
    }
    
    
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
            await _addressService.UpdateAsync(id, addressDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AddressDto>.FailureResponse(ex.Message));
        }
    }
}