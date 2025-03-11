using AutoMapper;
using BusinessObjects.Dto.Product;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly string _currentUser; 

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _currentUser = "System";
    }

    public async Task<ProductDto> GetByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
            throw new KeyNotFoundException($"Product with ID {id} not found or has been deleted.");

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<PagedResponse<ProductDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            pageNumber,
            pageSize,
            p => p.IsDeleted == false // Filter out deleted products
        );
        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
        return new PagedResponse<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto> CreateAsync(ProductForCreationDto productDto)
    {
        if (productDto == null)
            throw new ArgumentNullException(nameof(productDto), "Product data cannot be null.");

        var product = _mapper.Map<Product>(productDto);
        product.CreatedTime = DateTimeOffset.UtcNow;
        product.CreatedBy = _currentUser;
        product.IsDeleted = false;

        _unitOfWork.Products.Add(product);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(ProductForUpdateDto productDto)
    {
        if (productDto == null)
            throw new ArgumentNullException(nameof(productDto), "Product data cannot be null.");

        var product = await _unitOfWork.Products.GetByIdAsync(productDto.Id);
        if (product == null || product.IsDeleted)
            throw new KeyNotFoundException($"Product with ID {productDto.Id} not found or has been deleted.");

        product.LastUpdatedTime = DateTimeOffset.UtcNow;
        product.LastUpdatedBy = _currentUser;

        _mapper.Map(productDto, product);
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
            throw new KeyNotFoundException($"Product with ID {id} not found or has been deleted.");
        product.IsDeleted = true;
        product.DeletedTime = DateTimeOffset.UtcNow;
        product.DeletedBy = _currentUser;
        _unitOfWork.Products.Update(product); 
        await _unitOfWork.SaveChangesAsync();
    }
}