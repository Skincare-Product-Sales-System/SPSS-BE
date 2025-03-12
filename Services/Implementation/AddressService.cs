using AutoMapper;
using BusinessObjects.Dto.Address;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;

public class AddressService : IAddressService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddressService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<AddressDto> GetByIdAsync(Guid id)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        if (address == null || address.IsDeleted)
            throw new KeyNotFoundException($"Address with ID {id} not found.");
        return _mapper.Map<AddressDto>(address);
    }

    public async Task<PagedResponse<AddressDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (products, totalCount) = await _unitOfWork.Addresses.GetPagedAsync(pageNumber, pageSize, a => a.IsDeleted == false);
        var productDtos = _mapper.Map<IEnumerable<AddressDto>>(products);
        return new PagedResponse<AddressDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AddressDto> CreateAsync(AddressForCreationDto? addressForCreationDto)
    {
        if (addressForCreationDto is null)
            throw new ArgumentNullException(nameof(addressForCreationDto), "Address data cannot be null.");

        var address = _mapper.Map<Address>(addressForCreationDto);
        address.CreatedTime = DateTimeOffset.UtcNow;
        address.CreatedBy = "System"; 
        address.IsDeleted = false;
        _unitOfWork.Addresses.Add(address);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<AddressDto>(address);
    }

    public async Task<AddressDto> UpdateAsync(Guid addressId, AddressForUpdateDto addressForUpdateDto)
    {
        if (addressForUpdateDto is null)
            throw new ArgumentNullException(nameof(addressForUpdateDto), "Address data cannot be null.");

        var address = await _unitOfWork.Addresses.GetByIdAsync(addressId);
        if (address == null)
            throw new KeyNotFoundException($"Address with ID {addressId} not found.");

        address.LastUpdatedTime = DateTimeOffset.UtcNow;
        address.LastUpdatedBy = "System"; 

        _mapper.Map(addressForUpdateDto, address);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<AddressDto>(address);
    }

    public async Task DeleteAsync(Guid id)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        if (address == null)
            throw new KeyNotFoundException($"Address with ID {id} not found.");
        address.IsDeleted = true;
        address.DeletedTime = DateTimeOffset.UtcNow;
        address.DeletedBy = "System";
        _unitOfWork.Addresses.Update(address); 
        await _unitOfWork.SaveChangesAsync();
    }
}