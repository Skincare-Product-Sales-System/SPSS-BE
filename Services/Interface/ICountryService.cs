using BusinessObjects.Dto.Country;
using BusinessObjects.Models;

namespace Services.Interface;

public interface ICountryService
{
    Task<IEnumerable<CountryDto>> GetAllAsync();
}