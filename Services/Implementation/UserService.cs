using System.Text.Json;
using AutoMapper;
using BusinessObjects.Dto.User;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);

        if (user == null || user.IsDeleted)
            throw new KeyNotFoundException($"User with ID {id} not found.");

        return _mapper.Map<UserDto>(user);
    }
    
    public async Task<UserDto> GetByEmailAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user == null || user.IsDeleted)
            throw new KeyNotFoundException($"User with email {email} not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> GetByUserNameAsync(string userName)
    {
        var user = await _unitOfWork.Users.GetByUserNameAsync(userName);

        if (user == null || user.IsDeleted)
            throw new KeyNotFoundException($"User with user name {userName} not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<PagedResponse<UserDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (users, totalCount) = await _unitOfWork.Users.GetPagedAsync(
            pageNumber,
            pageSize,
            u => !u.IsDeleted // Only active users
        );

        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

        return new PagedResponse<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<UserDto> CreateAsync(UserForCreationDto? userForCreationDto)
    {
        
        // Log giá trị của UserForCreationDto
        Console.WriteLine($"UserName: {userForCreationDto.UserName}");
        Console.WriteLine($"SurName: {userForCreationDto.SurName}");
        Console.WriteLine($"LastName: {userForCreationDto.LastName}");
        Console.WriteLine($"EmailAddress: {userForCreationDto.EmailAddress}");
        Console.WriteLine($"PhoneNumber: {userForCreationDto.PhoneNumber}");
        Console.WriteLine($"Password: {userForCreationDto.Password}");
        
        
        if (userForCreationDto == null)
            throw new ArgumentNullException(nameof(userForCreationDto), "User data cannot be null.");

        var user = new User
        {
            UserId = Guid.NewGuid(), 
            SkinTypeId = userForCreationDto.SkinTypeId,
            RoleId = userForCreationDto.RoleId,
            UserName = userForCreationDto.UserName,
            SurName = userForCreationDto.SurName,
            LastName = userForCreationDto.LastName,
            EmailAddress = userForCreationDto.EmailAddress,
            PhoneNumber = userForCreationDto.PhoneNumber,
            Status = userForCreationDto.Status,
            Password = userForCreationDto.Password,
            AvatarUrl = userForCreationDto.AvatarUrl,
            CreatedBy = "System", 
            CreatedTime = DateTimeOffset.UtcNow,
            IsDeleted = false 
        };
        Console.WriteLine($"User before save: {JsonSerializer.Serialize(user)}");
        _unitOfWork.Users.Add(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateAsync(Guid userId, UserForUpdateDto userForUpdateDto)
    {
        if (userForUpdateDto == null)
            throw new ArgumentNullException(nameof(userForUpdateDto), "User data cannot be null.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null || user.IsDeleted)
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        user.LastUpdatedTime = DateTimeOffset.UtcNow;
        user.LastUpdatedBy = "System"; // Optionally replace with the current user if available

        _mapper.Map(userForUpdateDto, user);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);

        if (user == null || user.IsDeleted)
            throw new KeyNotFoundException($"User with ID {id} not found.");

        user.IsDeleted = true;
        user.DeletedTime = DateTimeOffset.UtcNow;
        user.DeletedBy = "System"; // Optionally replace with the current user if available

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> CheckUserNameExistsAsync(string userName)
    {
        try
        {
            await GetByUserNameAsync(userName);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    public async Task<bool> CheckEmailExistsAsync(string email)
    {
        try
        {
            await GetByEmailAsync(email);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }
}
