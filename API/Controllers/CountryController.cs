using BusinessObjects.Dto.Country;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace API.Controllers
{
    [ApiController]
    [Route("api/countries")]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService) => _countryService = countryService;
        
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var countries = await _countryService.GetAllAsync();
            return Ok(countries);
        }
    }
}