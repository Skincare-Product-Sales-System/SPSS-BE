using AutoMapper;
using BusinessObjects.Dto.User;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
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
        // Lấy thông tin user cùng với role
        var user = await _unitOfWork.Users
            .GetQueryable()
            .Include(u => u.Role) // Bao gồm thông tin Role
            .FirstOrDefaultAsync(u => u.EmailAddress == email);

        // Kiểm tra null
        if (user == null || user.IsDeleted)
            throw new KeyNotFoundException($"User with email {email} not found.");

        // Map thủ công từ User sang UserDto
        var userDto = new UserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            SurName = user.SurName,
            LastName = user.LastName,
            EmailAddress = user.EmailAddress,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status,
            Password = user.Password,
            SkinTypeId = user.SkinTypeId,
            RoleId = user.Role?.RoleId,
            Role = user.Role!.RoleName,
            CreatedBy = user.CreatedBy,
            LastUpdatedBy = user.LastUpdatedBy,
            DeletedBy = user.DeletedBy,
            CreatedTime = user.CreatedTime,
            LastUpdatedTime = user.LastUpdatedTime,
            DeletedTime = user.DeletedTime,
            IsDeleted = user.IsDeleted
        };

        return userDto;
    }

    public async Task<UserDto> GetByUserNameAsync(string userName)
    {
        // Lấy thông tin user cùng với role
        var user = await _unitOfWork.Users
            .GetQueryable()
            .Include(u => u.Role) // Bao gồm thông tin Role
            .FirstOrDefaultAsync(u => u.UserName == userName);

        // Kiểm tra null
        if (user == null || user.IsDeleted)
            throw new KeyNotFoundException($"User with user name {userName} not found.");

        // Map thủ công từ User sang UserDto
        var userDto = new UserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            SurName = user.SurName,
            LastName = user.LastName,
            EmailAddress = user.EmailAddress,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status,
            Password = user.Password, // Nếu trả về mật khẩu, hãy đảm bảo đã mã hóa
            SkinTypeId = user.SkinTypeId,
            RoleId = user.Role?.RoleId,
            Role = user.Role?.RoleName, // Gán tên vai trò từ bảng Role
            CreatedBy = user.CreatedBy,
            LastUpdatedBy = user.LastUpdatedBy,
            DeletedBy = user.DeletedBy,
            CreatedTime = user.CreatedTime,
            LastUpdatedTime = user.LastUpdatedTime,
            DeletedTime = user.DeletedTime,
            IsDeleted = user.IsDeleted
        };

        return userDto;
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
        if (userForCreationDto == null)
            throw new ArgumentNullException(nameof(userForCreationDto), "User data cannot be null.");

        var user = _mapper.Map<User>(userForCreationDto);

        user.CreatedTime = DateTimeOffset.UtcNow;
        user.CreatedBy = "System"; // Optionally replace with the current user if available
        user.IsDeleted = false;

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

   
}
