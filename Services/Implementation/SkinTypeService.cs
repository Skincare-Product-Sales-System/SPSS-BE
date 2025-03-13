using AutoMapper;
using BusinessObjects.Dto.SkinType;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;

public class SkinTypeService : ISkinTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SkinTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SkinTypeDto> GetByIdAsync(Guid id)
    {
        var skinType = await _unitOfWork.SkinTypes.GetByIdAsync(id);
        if (skinType == null || skinType.IsDeleted)
            throw new KeyNotFoundException($"SkinType with ID {id} not found.");
        return _mapper.Map<SkinTypeDto>(skinType);
    }

    public async Task<PagedResponse<SkinTypeDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (skinTypes, totalCount) = await _unitOfWork.SkinTypes.GetPagedAsync(
            pageNumber, pageSize, s => s.IsDeleted == false);

        var skinTypeDtos = _mapper.Map<IEnumerable<SkinTypeDto>>(skinTypes);
        return new PagedResponse<SkinTypeDto>
        {
            Items = skinTypeDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<SkinTypeDto> CreateAsync(SkinTypeForCreationDto? skinTypeForCreationDto)
    {
        if (skinTypeForCreationDto is null)
            throw new ArgumentNullException(nameof(skinTypeForCreationDto), "SkinType data cannot be null.");

        var skinType = _mapper.Map<SkinType>(skinTypeForCreationDto);
        skinType.CreatedTime = DateTimeOffset.UtcNow;
        skinType.CreatedBy = "System"; // You can replace "System" with actual user context
        skinType.IsDeleted = false;

        _unitOfWork.SkinTypes.Add(skinType);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<SkinTypeDto>(skinType);
    }

    public async Task<SkinTypeDto> UpdateAsync(Guid skinTypeId, SkinTypeForUpdateDto skinTypeForUpdateDto)
    {
        if (skinTypeForUpdateDto is null)
            throw new ArgumentNullException(nameof(skinTypeForUpdateDto), "SkinType data cannot be null.");

        var skinType = await _unitOfWork.SkinTypes.GetByIdAsync(skinTypeId);
        if (skinType == null || skinType.IsDeleted)
            throw new KeyNotFoundException($"SkinType with ID {skinTypeId} not found.");

        skinType.LastUpdatedTime = DateTimeOffset.UtcNow;
        skinType.LastUpdatedBy = "System"; // You can replace "System" with actual user context

        _mapper.Map(skinTypeForUpdateDto, skinType);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<SkinTypeDto>(skinType);
    }

    public async Task DeleteAsync(Guid id)
    {
        var skinType = await _unitOfWork.SkinTypes.GetByIdAsync(id);
        if (skinType == null || skinType.IsDeleted)
            throw new KeyNotFoundException($"SkinType with ID {id} not found.");

        skinType.IsDeleted = true;
        skinType.DeletedTime = DateTimeOffset.UtcNow;
        skinType.DeletedBy = "System"; // You can replace "System" with actual user context

        _unitOfWork.SkinTypes.Update(skinType);
        await _unitOfWork.SaveChangesAsync();
    }
}
