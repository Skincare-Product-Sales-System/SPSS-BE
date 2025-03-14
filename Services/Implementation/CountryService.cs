using AutoMapper;
using BusinessObjects.Dto.Country;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;

        public CountryService(ICountryRepository countryRepository, IMapper mapper)
        {
            _countryRepository = countryRepository;
            _mapper = mapper;
        }

        public async Task<CountryDto> GetByIdAsync(int id)
        {
            var country = await _countryRepository.GetByIdAsync(id);
            if (country == null)
                throw new KeyNotFoundException($"Country with ID {id} not found.");

            return _mapper.Map<CountryDto>(country);
        }

        public async Task<IEnumerable<CountryDto>> GetAllAsync()
        {
            var countries = await _countryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CountryDto>>(countries);
        }

        public async Task<CountryDto> CreateAsync(CountryForCreationDto countryForCreationDto)
        {
            if (countryForCreationDto == null)
                throw new ArgumentNullException(nameof(countryForCreationDto), "Country data cannot be null.");

            var country = _mapper.Map<Country>(countryForCreationDto);
            await _countryRepository.AddAsync(country);

            return _mapper.Map<CountryDto>(country);
        }

        public async Task<CountryDto> UpdateAsync(int countryId, CountryForUpdateDto countryForUpdateDto)
        {
            if (countryForUpdateDto == null)
                throw new ArgumentNullException(nameof(countryForUpdateDto), "Country data cannot be null.");

            var country = await _countryRepository.GetByIdAsync(countryId);
            if (country == null)
                throw new KeyNotFoundException($"Country with ID {countryId} not found.");

            _mapper.Map(countryForUpdateDto, country);
            await _countryRepository.UpdateAsync(country);

            return _mapper.Map<CountryDto>(country);
        }

        public async Task DeleteAsync(int id)
        {
            var country = await _countryRepository.GetByIdAsync(id);
            if (country == null)
                throw new KeyNotFoundException($"Country with ID {id} not found.");

            await _countryRepository.DeleteAsync(id);
        }
    }
}
