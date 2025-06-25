using AutoMapper;
using BusinessObjects.Dto.Address;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using System;
using System.Linq;
using System.Threading.Tasks;

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

    public async Task<PagedResponse<AddressDto>> GetByUserIdPagedAsync(Guid userId, int pageNumber, int pageSize)
    {
        var query = _unitOfWork.Addresses.Entities
            .Include(a => a.User)        
            .Include(a => a.Country)     
            .Where(a => a.UserId == userId && !a.IsDeleted);

        int totalCount = await query.CountAsync();

        var allAddresses = await query
            .OrderBy(a => a.CreatedTime)  
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var addressDtos = allAddresses.Select(a => new AddressDto
        {
            Id = a.Id,
            CountryId = a.CountryId,
            CustomerName = a.CustomerName,
            IsDefault = a.IsDefault,
            PhoneNumber = a.PhoneNumber,
            CountryName = a.Country?.CountryName ?? "Unknown",
            StreetNumber = a.StreetNumber,
            AddressLine1 = a.AddressLine1,
            AddressLine2 = a.AddressLine2,
            City = a.City,
            Ward = a.Ward,
            PostCode = a.Postcode,
            Province = a.Province
        }).ToList();

        return new PagedResponse<AddressDto>
        {
            Items = addressDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AddressDto> CreateAsync(AddressForCreationDto? addressForCreationDto, Guid userId)
    {
        if (addressForCreationDto is null)
            throw new ArgumentNullException(nameof(addressForCreationDto), "Address data cannot be null.");

        // Validate that the country exists
        var countryExists = await _unitOfWork.Countries.Entities
            .AnyAsync(c => c.Id == addressForCreationDto.CountryId);
        
        if (!countryExists)
            throw new ArgumentException($"Country with ID {addressForCreationDto.CountryId} does not exist.", nameof(addressForCreationDto.CountryId));

        // Validate user exists
        var userExists = await _unitOfWork.Users.Entities
            .AnyAsync(u => u.UserId == userId);
            
        if (!userExists)
            throw new ArgumentException($"User with ID {userId} does not exist.", nameof(userId));

        // Thực hiện ánh xạ thủ công từ AddressForCreationDto sang Address
        var address = new Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CountryId = addressForCreationDto.CountryId,
            CustomerName = addressForCreationDto.CustomerName,
            PhoneNumber = addressForCreationDto.PhoneNumber,
            StreetNumber = addressForCreationDto.StreetNumber,
            AddressLine1 = addressForCreationDto.AddressLine1,
            AddressLine2 = addressForCreationDto.AddressLine2,
            City = addressForCreationDto.City,
            Ward = addressForCreationDto.Ward,
            Postcode = addressForCreationDto.Postcode,
            Province = addressForCreationDto.Province,
            CreatedTime = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastUpdatedBy = userId.ToString(),
            LastUpdatedTime = DateTimeOffset.UtcNow,
            IsDeleted = false,
            IsDefault = addressForCreationDto.IsDefault
        };

        // Xử lý thiết lập IsDefault
        if (addressForCreationDto.IsDefault)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                // Lấy danh sách địa chỉ hiện tại của người dùng
                var userAddresses = await _unitOfWork.Addresses.Entities
                    .Where(a => a.UserId == userId && !a.IsDeleted)
                    .ToListAsync();

                // Đặt tất cả các địa chỉ hiện tại không còn là mặc định
                foreach (var userAddress in userAddresses)
                {
                    if (userAddress.IsDefault)
                    {
                        userAddress.IsDefault = false;
                        _unitOfWork.Addresses.Update(userAddress);
                    }
                }
                
                // Thêm địa chỉ mới vào cơ sở dữ liệu
                _unitOfWork.Addresses.Add(address);
                
                // Lưu các thay đổi
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Failed to create address: {ex.Message}", ex);
            }
        }
        else
        {
            // Thêm địa chỉ mới vào cơ sở dữ liệu (không cần transaction nếu không phải địa chỉ mặc định)
            _unitOfWork.Addresses.Add(address);
            await _unitOfWork.SaveChangesAsync();
        }

        // Lấy dữ liệu Address cùng các liên quan (Country, User)
        var savedAddress = await _unitOfWork.Addresses.Entities
            .Include(a => a.Country) // Include Country
            .Include(a => a.User)    // Include User
            .FirstOrDefaultAsync(a => a.Id == address.Id);

        if (savedAddress == null)
            throw new Exception("Failed to retrieve the saved address.");

        // Thực hiện ánh xạ thủ công từ Address sang AddressDto
        var addressDto = new AddressDto
        {
            Id = savedAddress.Id,
            CountryId = savedAddress.CountryId,
            CountryName = savedAddress.Country?.CountryName ?? "Unknown",
            CustomerName = savedAddress.CustomerName,
            PhoneNumber = savedAddress.PhoneNumber,
            StreetNumber = savedAddress.StreetNumber,
            AddressLine1 = savedAddress.AddressLine1,
            AddressLine2 = savedAddress.AddressLine2,
            City = savedAddress.City,
            Ward = savedAddress.Ward,
            PostCode = savedAddress.Postcode,
            Province = savedAddress.Province,
            IsDefault = savedAddress.IsDefault
        };

        return addressDto;
    }

    public async Task<bool> UpdateAsync(Guid addressId, AddressForUpdateDto addressForUpdateDto, Guid userId)
    {
        if (addressForUpdateDto is null)
            throw new ArgumentNullException(nameof(addressForUpdateDto), "Address data cannot be null.");

        // Validate that the country exists
        var countryExists = await _unitOfWork.Countries.Entities
            .AnyAsync(c => c.Id == addressForUpdateDto.CountryId);
        
        if (!countryExists)
            throw new ArgumentException($"Country with ID {addressForUpdateDto.CountryId} does not exist.", nameof(addressForUpdateDto.CountryId));

        var address = await _unitOfWork.Addresses.GetByIdAsync(addressId);
        if (address == null)
            throw new KeyNotFoundException($"Address with ID {addressId} not found.");

        // Cập nhật thông tin từ DTO sang entity
        address.CountryId = addressForUpdateDto.CountryId;
        address.StreetNumber = addressForUpdateDto.StreetNumber;
        address.AddressLine1 = addressForUpdateDto.AddressLine1;
        address.CustomerName = addressForUpdateDto.CustomerName;
        address.PhoneNumber = addressForUpdateDto.PhoneNumber;
        address.AddressLine2 = addressForUpdateDto.AddressLine2;
        address.City = addressForUpdateDto.City;
        address.Ward = addressForUpdateDto.Ward;
        address.Postcode = addressForUpdateDto.Postcode;
        address.Province = addressForUpdateDto.Province;
        address.LastUpdatedTime = DateTimeOffset.UtcNow;
        address.LastUpdatedBy = userId.ToString();

        _unitOfWork.Addresses.Update(address);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetAsDefaultAsync(Guid addressId, Guid userId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // Lấy danh sách địa chỉ của người dùng
            var userAddresses = await _unitOfWork.Addresses.Entities
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .ToListAsync();

            // Kiểm tra xem địa chỉ cần đặt mặc định có tồn tại không
            var addressToSetDefault = userAddresses.FirstOrDefault(a => a.Id == addressId);
            if (addressToSetDefault == null)
                throw new KeyNotFoundException($"Address with ID {addressId} not found for the current user.");

            // Kiểm tra nếu địa chỉ đã là mặc định, không cần thực hiện thay đổi
            if (addressToSetDefault.IsDefault)
                return true;

            // Bỏ mặc định địa chỉ hiện tại (nếu có)
            foreach (var address in userAddresses)
            {
                if (address.IsDefault)
                {
                    address.IsDefault = false;
                    _unitOfWork.Addresses.Update(address);
                }
            }

            // Đặt địa chỉ yêu cầu thành mặc định
            addressToSetDefault.IsDefault = true;
            _unitOfWork.Addresses.Update(addressToSetDefault);
            
            // Lưu thay đổi
            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new Exception($"Failed to set address as default: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(id);
        if (address == null)
            throw new KeyNotFoundException($"Address with ID {id} not found.");
        
        address.DeletedBy = userId.ToString();
        address.DeletedTime = DateTimeOffset.UtcNow;
        address.IsDeleted = true;

        _unitOfWork.Addresses.Update(address); 
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}