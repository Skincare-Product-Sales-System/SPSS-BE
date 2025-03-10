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

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ProductDto> GetByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found.");

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<PagedResponse<ProductDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(pageNumber, pageSize);
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
        _unitOfWork.Products.Add(product);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(ProductForUpdateDto productDto)
    {
        if (productDto == null)
            throw new ArgumentNullException(nameof(productDto), "Product data cannot be null.");
        var product = await _unitOfWork.Products.GetByIdAsync(productDto.Id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {productDto.Id} not found.");
        _mapper.Map(productDto, product); 
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(); 
        return _mapper.Map<ProductDto>(product); 
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync();
    }
}