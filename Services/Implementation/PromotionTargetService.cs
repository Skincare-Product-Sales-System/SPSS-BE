using AutoMapper;
using BusinessObjects.Dto.PromotionTarget;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;

public class PromotionTargetService : IPromotionTargetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PromotionTargetService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PromotionTargetDto> GetByIdAsync(Guid id)
    {
        var promotionTarget = await _unitOfWork.PromotionTargets.GetByIdAsync(id);
        if (promotionTarget == null || promotionTarget.IsDeleted)
            throw new KeyNotFoundException($"PromotionTarget with ID {id} not found.");
        return _mapper.Map<PromotionTargetDto>(promotionTarget);
    }

    public async Task<PagedResponse<PromotionTargetDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (promotionTargets, totalCount) = await _unitOfWork.PromotionTargets.GetPagedAsync(
            pageNumber, pageSize, pt => pt.IsDeleted == false);

        var promotionTargetDtos = _mapper.Map<IEnumerable<PromotionTargetDto>>(promotionTargets);
        return new PagedResponse<PromotionTargetDto>
        {
            Items = promotionTargetDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PromotionTargetDto> CreateAsync(PromotionTargetForCreationDto? promotionTargetForCreationDto)
    {
        if (promotionTargetForCreationDto is null)
            throw new ArgumentNullException(nameof(promotionTargetForCreationDto), "PromotionTarget data cannot be null.");

        // Validate that at least one target is specified (optional business rule)
        if (promotionTargetForCreationDto.BrandId == null && 
            promotionTargetForCreationDto.ProductCategoryId == null && 
            promotionTargetForCreationDto.ProductId == null)
            throw new ArgumentException("At least one target (Brand, ProductCategory, or Product) must be specified.");

        var promotionTarget = _mapper.Map<PromotionTarget>(promotionTargetForCreationDto);
        promotionTarget.Id = Guid.NewGuid();
        promotionTarget.CreatedTime = DateTimeOffset.UtcNow;
        promotionTarget.CreatedBy = "System"; // Replace with actual user context if available
        promotionTarget.IsDeleted = false;

        _unitOfWork.PromotionTargets.Add(promotionTarget);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<PromotionTargetDto>(promotionTarget);
    }

    public async Task<PromotionTargetDto> UpdateAsync(Guid promotionTargetId, PromotionTargetForUpdateDto promotionTargetForUpdateDto)
    {
        if (promotionTargetForUpdateDto is null)
            throw new ArgumentNullException(nameof(promotionTargetForUpdateDto), "PromotionTarget data cannot be null.");

        var promotionTarget = await _unitOfWork.PromotionTargets.GetByIdAsync(promotionTargetId);
        if (promotionTarget == null || promotionTarget.IsDeleted)
            throw new KeyNotFoundException($"PromotionTarget with ID {promotionTargetId} not found.");

        promotionTarget.LastUpdatedTime = DateTimeOffset.UtcNow;
        promotionTarget.LastUpdatedBy = "System"; // Replace with actual user context if available

        _mapper.Map(promotionTargetForUpdateDto, promotionTarget);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<PromotionTargetDto>(promotionTarget);
    }

    public async Task DeleteAsync(Guid id)
    {
        var promotionTarget = await _unitOfWork.PromotionTargets.GetByIdAsync(id);
        if (promotionTarget == null || promotionTarget.IsDeleted)
            throw new KeyNotFoundException($"PromotionTarget with ID {id} not found.");

        promotionTarget.IsDeleted = true;
        promotionTarget.DeletedTime = DateTimeOffset.UtcNow;
        promotionTarget.DeletedBy = "System"; // Replace with actual user context if available

        _unitOfWork.PromotionTargets.Update(promotionTarget);
        await _unitOfWork.SaveChangesAsync();
    }
}