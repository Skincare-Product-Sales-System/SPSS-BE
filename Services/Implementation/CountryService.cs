using AutoMapper;
using BusinessObjects.Dto.Country;
using Repositories.Interface;
using Services.Interface;

namespace Services.Implementation;

public class CountryService : ICountryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CountryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<IEnumerable<CountryDto>> GetAllAsync()
    {
        var countries = await _unitOfWork.Countries.GetAllAsync();
        return _mapper.Map<IEnumerable<CountryDto>>(countries);
    }
}