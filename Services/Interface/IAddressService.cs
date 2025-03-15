using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.Product;
using BusinessObjects.Models;
using Services.Response;

namespace Services.Interface;

public interface IAddressService
{
    Task<PagedResponse<AddressDto>> GetByUserIdPagedAsync(Guid userId, int pageNumber, int pageSize);
    Task<bool> CreateAsync(AddressForCreationDto? addressForCreationDto, Guid userId);
    Task<bool> UpdateAsync(Guid addressId, AddressForUpdateDto addressForUpdateDto, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<bool> SetAsDefaultAsync(Guid addressId, Guid userId);
}