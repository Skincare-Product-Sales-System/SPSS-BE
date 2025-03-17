using AutoMapper;
using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.SkincareRoutinStep;
using BusinessObjects.Dto.SkinType;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
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

    public async Task<SkinTypeWithDetailDto> GetByIdAsync(Guid id)
    {
        // Truy vấn dữ liệu với Include
        var skinType = await _unitOfWork.SkinTypes.Entities
            .Include(s => s.SkinTypeRoutineSteps) // Include các bước chăm sóc
                .ThenInclude(step => step.Category) // Include Category trong từng bước
            .Include(s => s.SkinTypeRoutineSteps) // Include thêm Products trong từng bướ
            .FirstOrDefaultAsync(s => s.Id == id); // Lọc theo ID

        if (skinType == null)
            throw new KeyNotFoundException($"SkinType with ID {id} not found.");

        // Ánh xạ thủ công
        var dto = new SkinTypeWithDetailDto
        {
            Id = skinType.Id,
            Name = skinType.Name,
            Description = skinType.Description,
            SkinTypeRoutineSteps = skinType.SkinTypeRoutineSteps
                .OrderBy(step => step.Order) // Đảm bảo thứ tự bước
                .Select(step => new SkinTypeRoutineStepDto
                {
                    StepName = step.StepName,
                    Instruction = step.Instruction,
                    Order = step.Order,
                    Category = step.Category == null ? null : new ProductCategoryOverviewDto
                    {
                        Id = step.Category.Id,
                        CategoryName = step.Category.CategoryName
                    },
                    Products = []
                }).ToList()
        };

        return dto;
    }

    public async Task<PagedResponse<SkinTypeDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        // Retrieve paged SkinType entities and total count
        var (skinTypes, totalCount) = await _unitOfWork.SkinTypes.GetPagedAsync(
            pageNumber, pageSize, null);

        // Manual mapping from SkinType to SkinTypeDto
        var skinTypeDtos = skinTypes.Select(skinType => new SkinTypeDto
        {
            Id = skinType.Id,
            Name = skinType.Name,
        }).ToList();

        // Create and return a paged response
        return new PagedResponse<SkinTypeDto>
        {
            Items = skinTypeDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> CreateAsync(SkinTypeForCreationDto? skinTypeForCreationDto, Guid userId)
    {
        if (skinTypeForCreationDto is null)
            throw new ArgumentNullException(nameof(skinTypeForCreationDto), "SkinType data cannot be null.");

        // Map SkinType thủ công
        var skinType = new SkinType
        {
            Id = Guid.NewGuid(),
            Name = skinTypeForCreationDto.Name,
            Description = skinTypeForCreationDto.Description,
        };

        // Handle Routine Steps
        if (skinTypeForCreationDto.SkinTypeRoutineSteps != null && skinTypeForCreationDto.SkinTypeRoutineSteps.Any())
        {
            // Map Routine Steps và gắn vào SkinType
            var routineSteps = skinTypeForCreationDto.SkinTypeRoutineSteps.Select(step => new SkinTypeRoutineStep
            {
                Id = Guid.NewGuid(),
                SkinTypeId = skinType.Id,
                StepName = step.StepName,
                Instruction = step.Instruction,
                CategoryId = step.CategoryId,
                Order = step.Order
            }).ToList();

            skinType.SkinTypeRoutineSteps = routineSteps;
        }

        try
        {
            _unitOfWork.SkinTypes.Add(skinType);
            await _unitOfWork.SaveChangesAsync();
            // Thông báo thành công nếu cần
            Console.WriteLine("SkinType đã được thêm thành công vào database.");
        }
        catch (Exception ex)
        {
            // Ghi log lỗi hoặc xử lý lỗi tại đây
            Console.WriteLine($"Đã xảy ra lỗi khi thêm SkinType: {ex.Message}");
            // Có thể ném lại ngoại lệ nếu muốn
            throw;
        }
        return true;
    }

    public async Task<SkinTypeWithDetailDto> UpdateAsync(Guid skinTypeId, SkinTypeForUpdateDto skinTypeForUpdateDto)
    {
        if (skinTypeForUpdateDto is null)
            throw new ArgumentNullException(nameof(skinTypeForUpdateDto), "SkinType data cannot be null.");

        var skinType = await _unitOfWork.SkinTypes.GetByIdAsync(skinTypeId);
        if (skinType == null)
            throw new KeyNotFoundException($"SkinType with ID {skinTypeId} not found.");

        _mapper.Map(skinTypeForUpdateDto, skinType);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<SkinTypeWithDetailDto>(skinType);
    }

    public async Task DeleteAsync(Guid id)
    {
        var skinType = await _unitOfWork.SkinTypes.GetByIdAsync(id);
        if (skinType == null)
            throw new KeyNotFoundException($"SkinType with ID {id} not found.");

        _unitOfWork.SkinTypes.Update(skinType);
        await _unitOfWork.SaveChangesAsync();
    }
}
