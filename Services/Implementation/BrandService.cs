using AutoMapper;
using BusinessObjects.Dto.Brand;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;

public class BrandService : IBrandService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BrandService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BrandDto> GetByIdAsync(Guid id)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);

        if (brand == null || brand.IsDeleted)
            throw new KeyNotFoundException($"Brand with ID {id} not found.");

        return _mapper.Map<BrandDto>(brand);
    }

    public async Task<PagedResponse<BrandDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (brands, totalCount) = await _unitOfWork.Brands.GetPagedAsync(
            pageNumber,
            pageSize,
            b => !b.IsDeleted // Only active brands
        );

        var brandDtos = _mapper.Map<IEnumerable<BrandDto>>(brands);

        return new PagedResponse<BrandDto>
        {
            Items = brandDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<BrandDto> CreateAsync(BrandForCreationDto? brandForCreationDto, Guid userId)
    {
        if (brandForCreationDto == null)
            throw new ArgumentNullException(nameof(brandForCreationDto), "Brand data cannot be null.");

        // Manually map properties from DTO to the entity
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = brandForCreationDto.Name,
            Description = brandForCreationDto.Description,
            ImageUrl = brandForCreationDto.ImageUrl,
            CreatedTime = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            IsDeleted = false,
            LastUpdatedBy = userId.ToString(),
            LastUpdatedTime = DateTimeOffset.UtcNow
        };

        _unitOfWork.Brands.Add(brand);
        await _unitOfWork.SaveChangesAsync();

        var brandDto = new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            ImageUrl = brand.ImageUrl
        };

        return brandDto;
    }

    public async Task<BrandDto> UpdateAsync(Guid brandId, BrandForUpdateDto brandForUpdateDto, Guid userId)
    {
        if (brandForUpdateDto == null)
            throw new ArgumentNullException(nameof(brandForUpdateDto), "Brand data cannot be null.");

        // Retrieve the existing brand entity
        var brand = await _unitOfWork.Brands.GetByIdAsync(brandId);
        if (brand == null || brand.IsDeleted)
            throw new KeyNotFoundException($"Brand with ID {brandId} not found.");

        // Update properties manually
        brand.Name = brandForUpdateDto.Name;
        brand.Description = brandForUpdateDto.Description;
        brand.ImageUrl = brandForUpdateDto.ImageUrl;
        brand.LastUpdatedTime = DateTimeOffset.UtcNow;
        brand.LastUpdatedBy = userId.ToString();

        _unitOfWork.Brands.Update(brand);
        await _unitOfWork.SaveChangesAsync();

        // Manually map the updated entity to the DTO
        var brandDto = new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            ImageUrl = brand.ImageUrl
        };

        return brandDto;
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        // Fetch the brand entity by ID
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand == null || brand.IsDeleted)
            throw new KeyNotFoundException($"Brand with ID {id} not found.");

        // Update the properties manually for soft deletion
        brand.IsDeleted = true;
        brand.DeletedTime = DateTimeOffset.UtcNow;
        brand.DeletedBy = userId.ToString();

        // Save the changes
        _unitOfWork.Brands.Update(brand);
        await _unitOfWork.SaveChangesAsync();
    }
}
