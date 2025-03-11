using AutoMapper;
using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.Variation;
using BusinessObjects.Models;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using Shared.Constants;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IProductStatusService _productStatusService;
    private readonly string _currentUser;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IProductStatusService productStatusService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _productStatusService = productStatusService;
    }

    public async Task<ProductWithDetailsDto> GetByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products
            .GetQueryable()
            .Include(p => p.ProductCategory)
            .Include(p => p.ProductItems)
                .ThenInclude(pi => pi.ProductConfigurations)
                    .ThenInclude(pc => pc.VariationOption)
                        .ThenInclude(vo => vo.Variation)
            .Include(p => p.Brand)  
            .Include(p => p.ProductImages) 
            .Include(p => p.PromotionTargets)
                .ThenInclude(pt => pt.Promotion) 
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found.");

        return _mapper.Map<ProductWithDetailsDto>(product);
    }

    public async Task<PagedResponse<ProductDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
                pageNumber,
                pageSize,
                cr => cr.IsDeleted == false
            );

        var orderedProducts = products.OrderByDescending(p => p.CreatedTime);

        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(orderedProducts);

        return new PagedResponse<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> CreateAsync(ProductForCreationDto productDto, string userId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var categoryExists = await _unitOfWork.ProductCategories.Entities
                .AnyAsync(c => c.Id == productDto.ProductCategoryId);
            if (!categoryExists)
            {
                throw new ArgumentNullException($"Category with ID {productDto.ProductCategoryId} not found.");
            }

            // Step 5: Map the product DTO to the Product entity
            var productEntity = _mapper.Map<Product>(productDto);
            foreach (var item in productEntity.ProductItems)
            {
                if (item.Id == Guid.Empty)
                {
                    item.Id = Guid.NewGuid();
                }
            }

            productEntity.Id = Guid.NewGuid();
            productEntity.CreatedTime = DateTime.UtcNow;
            productEntity.LastUpdatedTime = DateTime.UtcNow;
            // Map hình ảnh từ ProductImageUrls
            for (int i = 0; i < productDto.ProductImageUrls.Count; i++)
            {
                var imageUrl = productDto.ProductImageUrls[i];
                productEntity.ProductImages.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = productEntity.Id,
                    ImageUrl = imageUrl,
                    IsThumbnail = (i == 0),
                    CreatedBy = userId,
                    LastUpdatedBy = userId,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                });
            }

            productEntity.ProductStatusId = await _productStatusService.GetFirstAvailableProductStatusIdAsync();
            productEntity.CreatedBy = userId;
            productEntity.LastUpdatedBy = userId;
            _unitOfWork.Products.Add(productEntity);

            // Step 7: Validate if each Variation exists and if its ID is a valid GUID
            foreach (var variation in productDto.Variations)
            {

                var variationExists = await _unitOfWork.Variations.Entities
                    .AnyAsync(v => v.Id == variation.Id);
                if (!variationExists)
                {
                    throw new ArgumentNullException("The variation could not be found.");
                }
            }

            // Validate if each VariationOption belongs to the correct Variation and if its ID is a valid GUID
            foreach (var variation in productDto.Variations)
            {
                foreach (var variationOptionId in variation.VariationOptionIds)
                {
                    // Validate if the VariationOption belongs to the correct Variation
                    var variationOptionExists = await _unitOfWork.VariationOptions.Entities
                        .AnyAsync(vo => vo.Id == variationOptionId && vo.VariationId == variation.Id);

                    if (!variationOptionExists)
                    {
                        throw new ArgumentException("The variation option with ID {0} does not belong to variation {1}.");
                    }
                }
            }

            // Step 8: Validate if each VariationOption exists and if its ID is a valid GUID
            foreach (var variation in productDto.Variations)
            {
                foreach (var variationOptionId in variation.VariationOptionIds)
                {
                    var variationOptionExists = await _unitOfWork.VariationOptions.Entities
                        .AnyAsync(vo => vo.Id == variationOptionId);
                    if (!variationOptionExists)
                    {
                        throw new ArgumentException("Some variation options could not be found.");
                    }
                }
            }

            // Step 9: Handle the variations and collect VariationOptionIds per variation
            var variationOptionIdsPerVariation = new Dictionary<Guid, List<Guid>>();
            foreach (var variation in productDto.Variations)
            {
                variationOptionIdsPerVariation[variation.Id] = variation.VariationOptionIds;
            }

            // Step 10: Generate all combinations of VariationOptionIds
            var variationCombinations = GetVariationOptionCombinations(variationOptionIdsPerVariation);

            // Step 11: Check if all required combinations exist in VariationCombinations
            var providedCombinations = productDto.ProductItems
                .Select(vc => string.Join("-", vc.VariationOptionIds.OrderBy(id => id)))
                .ToList();

            var validCombinations = variationCombinations
                .Select(vc => string.Join("-", vc.VariationOptionIds.OrderBy(id => id)))
                .ToList();

            // Step 12: Compare combinations
            if (!validCombinations.All(providedCombinations.Contains))
            {
                throw new ArgumentException(nameof(productDto), "The combination is missing options from other variations.");
            }

            // Step 13: Create ProductItems and ProductConfigurations for each VariationCombination
            await AddVariationOptionsToProduct(productEntity, productDto.ProductItems, userId);

            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task AddVariationOptionsToProduct(Product product, List<VariationCombinationDto> variationCombinations, string userId)
    {
        foreach (var combination in variationCombinations)
        {
            // Ensure VariationOptionIds are valid
            if (combination.VariationOptionIds == null || !combination.VariationOptionIds.Any())
            {
                throw new ArgumentException("VariationOptionIds cannot be null or empty.");
            }

            // Create a new ProductItem
            var productItem = new ProductItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Price = combination.Price,
                QuantityInStock = combination.QuantityInStock,
                ImageUrl = combination.ImageUrl,
                CreatedBy = userId,
            };

            // Add ProductItem to the DbContext
            _unitOfWork.ProductItems.Add(productItem);

            // Create ProductConfigurations for each VariationOptionId
            foreach (var variationOptionId in combination.VariationOptionIds)
            {
                var productConfiguration = new ProductConfiguration
                {
                    Id = Guid.NewGuid(),
                    ProductItemId = productItem.Id,
                    VariationOptionId = variationOptionId,
                    CreatedBy = userId,
                };

                // Add ProductConfiguration to the DbContext
                _unitOfWork.ProductConfigurations.Add(productConfiguration);
            }
        }
    }

    // Method to generate all combinations of VariationOptionIds
    private List<VariationCombinationDto> GetVariationOptionCombinations(Dictionary<Guid, List<Guid>> variationOptionIdsPerVariation)
    {
        var allCombinations = new List<VariationCombinationDto>();

        // Generate all combinations from the variation options
        var lists = variationOptionIdsPerVariation.Values.ToList();
        var combinations = GetCombinations(lists);

        foreach (var combination in combinations)
        {
            // Ensure unique combinations
            if (!allCombinations.Any(c => c.VariationOptionIds.SequenceEqual(combination)))
            {
                allCombinations.Add(new VariationCombinationDto
                {
                    VariationOptionIds = combination
                });
            }
        }

        return allCombinations;
    }

    private IEnumerable<List<Guid>> GetCombinations(List<List<Guid>> lists)
    {
        IEnumerable<IEnumerable<Guid>> result = new List<List<Guid>> { new List<Guid>() };

        foreach (var list in lists)
        {
            result = from combination in result
                     from item in list
                     select combination.Concat(new[] { item });
        }

        return result.Select(c => c.ToList());
    }

    public async Task<ProductDto> UpdateAsync(ProductForUpdateDto productDto, string userId)
    {
        if (productDto == null)
            throw new ArgumentNullException(nameof(productDto), "Product data cannot be null.");

        var product = await _unitOfWork.Products.GetByIdAsync(productDto.Id);
        if (product == null || product.IsDeleted)
            throw new KeyNotFoundException($"Product with ID {productDto.Id} not found or has been deleted.");

        product.LastUpdatedTime = DateTimeOffset.UtcNow;
        product.LastUpdatedBy = userId;

        _mapper.Map(productDto, product);
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(Guid id , string userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
            throw new KeyNotFoundException($"Product with ID {id} not found or has been deleted.");
        product.IsDeleted = true;
        product.DeletedTime = DateTimeOffset.UtcNow;
        product.DeletedBy = userId;
        _unitOfWork.Products.Update(product); 
        await _unitOfWork.SaveChangesAsync();
    }
}