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

    public async Task<BrandDto> CreateAsync(BrandForCreationDto? brandForCreationDto)
    {
        if (brandForCreationDto == null)
            throw new ArgumentNullException(nameof(brandForCreationDto), "Brand data cannot be null.");

        var brand = _mapper.Map<Brand>(brandForCreationDto);

        brand.Id = Guid.NewGuid(); 
        brand.CreatedTime = DateTimeOffset.UtcNow;
        brand.CreatedBy = "System"; 
        brand.IsDeleted = false;

        _unitOfWork.Brands.Add(brand);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BrandDto>(brand);
    }

    public async Task<BrandDto> UpdateAsync(Guid brandId, BrandForUpdateDto brandForUpdateDto)
    {
        if (brandForUpdateDto == null)
            throw new ArgumentNullException(nameof(brandForUpdateDto), "Brand data cannot be null.");
        var brand = await _unitOfWork.Brands.GetByIdAsync(brandId);
        if (brand == null || brand.IsDeleted)
            throw new KeyNotFoundException($"Brand with ID {brandId} not found.");
        brand.LastUpdatedTime = DateTimeOffset.UtcNow;
        brand.LastUpdatedBy = "System"; 
        _mapper.Map(brandForUpdateDto, brand);
        _unitOfWork.Brands.Update(brand);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<BrandDto>(brand);
    }

    public async Task DeleteAsync(Guid id)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand == null || brand.IsDeleted)
            throw new KeyNotFoundException($"Brand with ID {id} not found.");
        brand.IsDeleted = true;
        brand.DeletedTime = DateTimeOffset.UtcNow;
        brand.DeletedBy = "System";
        _unitOfWork.Brands.Update(brand);
        await _unitOfWork.SaveChangesAsync();
    }
}
