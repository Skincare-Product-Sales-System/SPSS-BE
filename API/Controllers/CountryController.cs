using BusinessObjects.Dto.Country;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Response;
using Services.Dto.Api;
using System.ComponentModel.DataAnnotations;
using API.Extensions;

namespace API.Controllers;

[ApiController]
[Route("api/countries")]
public class CountryController : ControllerBase
{
    private readonly ICountryService _countryService;

    public CountryController(ICountryService countryService)
    {
        _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
    }

    // ✅ Lấy thông tin country theo ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var country = await _countryService.GetByIdAsync(id);
        return country != null
            ? Ok(ApiResponse<CountryDto>.SuccessResponse(country))
            : NotFound(ApiResponse<CountryDto>.FailureResponse($"Country with ID {id} not found."));
    }

    // ✅ Lấy toàn bộ danh sách country
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var countries = await _countryService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<CountryDto>>.SuccessResponse(countries));
    }

    [CustomAuthorize("Manager")]
    // ✅ Tạo mới country
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CountryForCreationDto countryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CountryDto>.FailureResponse("Invalid country data", ModelStateErrors()));

        var createdCountry = await _countryService.CreateAsync(countryDto);
        return CreatedAtAction(nameof(GetById), new { id = createdCountry.Id }, ApiResponse<CountryDto>.SuccessResponse(createdCountry));
    }

    // ✅ Cập nhật country (partial update)
    [CustomAuthorize("Manager")]
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CountryForUpdateDto countryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CountryDto>.FailureResponse("Invalid country data", ModelStateErrors()));

        var country = await _countryService.UpdateAsync(id, countryDto);
        return country != null
            ? NoContent()
            : NotFound(ApiResponse<CountryDto>.FailureResponse($"Country with ID {id} not found."));
    }

    // ✅ Xóa country theo ID
    [CustomAuthorize("Manager")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var country = await _countryService.GetByIdAsync(id);
        if (country == null)
            return NotFound(ApiResponse<CountryDto>.FailureResponse($"Country with ID {id} not found."));

        await _countryService.DeleteAsync(id);
        return NoContent();
    }

    // ✅ Phương thức hỗ trợ lấy lỗi từ ModelState
    private List<string> ModelStateErrors()
    {
        return ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
    }
}
