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

        if (brand == null)
            throw new KeyNotFoundException($"Brand with ID {id} not found.");

        return _mapper.Map<BrandDto>(brand);
    }

    public async Task<PagedResponse<BrandDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (brands, totalCount) = await _unitOfWork.Brands.GetPagedAsync(
            pageNumber,
            pageSize,
            null
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
            CountryId = brandForCreationDto.CountryId,
            Description = brandForCreationDto.Description,
            ImageUrl = brandForCreationDto.ImageUrl,
            Title = brandForCreationDto.Title
        };

        _unitOfWork.Brands.Add(brand);
        await _unitOfWork.SaveChangesAsync();

        var brandDto = new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            ImageUrl = brand.ImageUrl,
            Title = brand.Title,
            CountryId = brand.CountryId
            
        };

        return brandDto;
    }

    public async Task<BrandDto> UpdateAsync(Guid brandId, BrandForUpdateDto brandForUpdateDto, Guid userId)
    {
        if (brandForUpdateDto == null)
            throw new ArgumentNullException(nameof(brandForUpdateDto), "Brand data cannot be null.");

        // Retrieve the existing brand entity
        var brand = await _unitOfWork.Brands.GetByIdAsync(brandId);
        if (brand == null)
            throw new KeyNotFoundException($"Brand with ID {brandId} not found.");

        // Update properties manually
        brand.Name = brandForUpdateDto.Name;
        brand.Description = brandForUpdateDto.Description;
        brand.ImageUrl = brandForUpdateDto.ImageUrl;
        brand.Title = brandForUpdateDto.Title;
        brand.CountryId = brandForUpdateDto.CountryId;

        _unitOfWork.Brands.Update(brand);
        await _unitOfWork.SaveChangesAsync();

        // Manually map the updated entity to the DTO
        var brandDto = new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            ImageUrl = brand.ImageUrl,
            Title = brand.Title,
            CountryId = brand.CountryId
        };

        return brandDto;
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        // Fetch the brand entity by ID
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand == null)
            throw new KeyNotFoundException($"Brand with ID {id} not found.");

        // Save the changes
        _unitOfWork.Brands.Delete(brand);
        await _unitOfWork.SaveChangesAsync();
    }
}
