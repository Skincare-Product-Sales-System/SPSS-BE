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
