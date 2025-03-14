using AutoMapper;
using BusinessObjects.Dto.Promotion;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;

public class PromotionService : IPromotionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PromotionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PromotionDto> GetByIdAsync(Guid id)
    {
        var promotion = await _unitOfWork.Promotions.GetByIdAsync(id);
        if (promotion == null || promotion.IsDeleted)
            throw new KeyNotFoundException($"Promotion with ID {id} not found.");
        return _mapper.Map<PromotionDto>(promotion);
    }

    public async Task<PagedResponse<PromotionDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (promotions, totalCount) = await _unitOfWork.Promotions.GetPagedAsync(
            pageNumber, pageSize, p => p.IsDeleted == false);

        var promotionDtos = _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        return new PagedResponse<PromotionDto>
        {
            Items = promotionDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PromotionDto> CreateAsync(PromotionForCreationDto? promotionForCreationDto)
    {
        if (promotionForCreationDto is null)
            throw new ArgumentNullException(nameof(promotionForCreationDto), "Promotion data cannot be null.");

        var promotion = _mapper.Map<Promotion>(promotionForCreationDto);
        promotion.Id = Guid.NewGuid();
        promotion.CreatedTime = DateTimeOffset.UtcNow;
        promotion.CreatedBy = "System"; // You can replace "System" with actual user context
        promotion.IsDeleted = false;

        _unitOfWork.Promotions.Add(promotion);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<PromotionDto>(promotion);
    }

    public async Task<PromotionDto> UpdateAsync(Guid promotionId, PromotionForUpdateDto promotionForUpdateDto)
    {
        if (promotionForUpdateDto is null)
            throw new ArgumentNullException(nameof(promotionForUpdateDto), "Promotion data cannot be null.");

        var promotion = await _unitOfWork.Promotions.GetByIdAsync(promotionId);
        if (promotion == null || promotion.IsDeleted)
            throw new KeyNotFoundException($"Promotion with ID {promotionId} not found.");

        promotion.LastUpdatedTime = DateTimeOffset.UtcNow;
        promotion.LastUpdatedBy = "System"; // You can replace "System" with actual user context

        _mapper.Map(promotionForUpdateDto, promotion);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<PromotionDto>(promotion);
    }

    public async Task DeleteAsync(Guid id)
    {
        var promotion = await _unitOfWork.Promotions.GetByIdAsync(id);
        if (promotion == null || promotion.IsDeleted)
            throw new KeyNotFoundException($"Promotion with ID {id} not found.");

        promotion.IsDeleted = true;
        promotion.DeletedTime = DateTimeOffset.UtcNow;
        promotion.DeletedBy = "System"; // You can replace "System" with actual user context

        _unitOfWork.Promotions.Update(promotion);
        await _unitOfWork.SaveChangesAsync();
    }
}