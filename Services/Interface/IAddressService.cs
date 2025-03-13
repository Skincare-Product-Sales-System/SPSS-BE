using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.Product;
using Services.Response;

namespace Services.Interface;

public interface IAddressService
{
    Task<AddressDto> GetByIdAsync(Guid id);
    Task<PagedResponse<AddressDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<AddressDto> CreateAsync(AddressForCreationDto? addressForCreationDto);
    Task<AddressDto> UpdateAsync(Guid addressId, AddressForUpdateDto addressForUpdateDto);
    Task DeleteAsync(Guid id);
}