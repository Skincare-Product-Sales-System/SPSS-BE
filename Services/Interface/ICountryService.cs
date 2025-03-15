using BusinessObjects.Dto.Country;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ICountryService
    {
        Task<CountryDto> GetByIdAsync(int id);
        Task<IEnumerable<CountryDto>> GetAllAsync();
        Task<CountryDto> CreateAsync(CountryForCreationDto countryForCreationDto);
        Task<CountryDto> UpdateAsync(int countryId, CountryForUpdateDto countryForUpdateDto);
        Task DeleteAsync(int id);
    }
}
