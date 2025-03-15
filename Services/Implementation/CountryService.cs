using AutoMapper;
using BusinessObjects.Dto.Country;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;

namespace Services.Implementation
{
    public class CountryService : ICountryService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CountryService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<CountryDto> GetByIdAsync(int id)
        {
            var country = await _unitOfWork.Countries.Entities.FirstOrDefaultAsync(c => c.Id == id);
            if (country == null)
                throw new KeyNotFoundException($"Country with ID {id} not found.");

            return _mapper.Map<CountryDto>(country);
        }

        public async Task<IEnumerable<CountryDto>> GetAllAsync()
        {
            var countries = await _unitOfWork.Countries.Entities.ToListAsync();
            return _mapper.Map<IEnumerable<CountryDto>>(countries);
        }

        public async Task<CountryDto> CreateAsync(CountryForCreationDto countryForCreationDto)
        {
            if (countryForCreationDto == null)
                throw new ArgumentNullException(nameof(countryForCreationDto), "Country data cannot be null.");

            var country = _mapper.Map<Country>(countryForCreationDto);
            _unitOfWork.Countries.Add(country);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CountryDto>(country);
        }

        public async Task<CountryDto> UpdateAsync(int countryId, CountryForUpdateDto countryForUpdateDto)
        {
            if (countryForUpdateDto == null)
                throw new ArgumentNullException(nameof(countryForUpdateDto), "Country data cannot be null.");

            // Check if the country exists
            var country = await _unitOfWork.Countries.Entities.FirstOrDefaultAsync(c => c.Id == countryId);
            if (country == null)
                throw new KeyNotFoundException($"Country with ID {countryId} not found.");

            // Map the updated fields to the existing entity
            _mapper.Map(countryForUpdateDto, country);

            // Update the entity in the repository
            _unitOfWork.Countries.Update(country);

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CountryDto>(country);
        }

        public async Task DeleteAsync(int id)
        {
            var country = await _unitOfWork.Countries.Entities.FirstOrDefaultAsync(c => c.Id == id);
            if (country == null)
                throw new KeyNotFoundException($"Country with ID {id} not found.");

            _unitOfWork.Countries.Delete(country);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
