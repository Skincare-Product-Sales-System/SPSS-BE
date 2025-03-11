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

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

    public async Task<ProductWithDetailsDto> CreateAsync(ProductForCreationDto productDto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            //var categoryExists = await _unitOfWork.ProducsCategories.Entities
            //    .AnyAsync(c => c.Id == productDto.ProductCategoryId);
            //if (!categoryExists)
            //{
            //    throw new ArgumentNullException($"Category with ID {productDto.ProductCategoryId} not found.");
            //}

            // Step 5: Map the product DTO to the Product entity
            var productEntity = _mapper.Map<Product>(productDto);
            //productEntity.ProductStatusId = ProductForStatus.Available;
            //productEntity.CreatedBy = userId;
            //productEntity.LastUpdatedBy = userId;

            //await _unitOfWork.ProductCategories.Add(productEntity);
            await _unitOfWork.SaveChangesAsync();

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
            var providedCombinations = productDto.VariationCombinations
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
            //await AddVariationOptionsToProduct(productEntity, productDto.VariationCombinations, userId);
            await AddVariationOptionsToProduct(productEntity, productDto.VariationCombinations);

            await _unitOfWork.CommitTransactionAsync();
            return _mapper.Map<ProductWithDetailsDto>(productEntity);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task AddVariationOptionsToProduct(Product product, List<VariationCombinationDto> variationCombinations)
    {
        foreach (var combination in variationCombinations)
        {
            var productItem = new ProductItem
            {
                ProductId = product.Id,
                Price = combination.Price,
                QuantityInStock = combination.QuantityInStock,
                //CreatedBy = userId,
                //LastUpdatedBy = userId
            };

            _unitOfWork.ProductItems.Add(productItem);
            await _unitOfWork.SaveChangesAsync();

            foreach (var variationOptionId in combination.VariationOptionIds)
            {
                var productConfiguration = new ProductConfiguration
                {
                    ProductItemId = productItem.Id,
                    VariationOptionId = variationOptionId
                };

                _unitOfWork.ProductConfigurations.Add(productConfiguration);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }

    //public async Task AddVariationOptionsToProduct(Product product, List<VariationCombinationDto> variationCombinations, string userId)
    //{
    //    foreach (var combination in variationCombinations)
    //    {
    //        var productItem = new ProductItem
    //        {
    //            ProductId = product.Id,
    //            Price = combination.Price,
    //            QuantityInStock = combination.QuantityInStock,
    //            CreatedBy = userId,
    //            LastUpdatedBy = userId
    //        };

    //        await _unitOfWork.GetRepository<ProductItem>().InsertAsync(productItem);
    //        await _unitOfWork.SaveAsync();

    //        foreach (var variationOptionId in combination.VariationOptionIds)
    //        {
    //            var productConfiguration = new ProductConfiguration
    //            {
    //                ProductItemId = productItem.Id,
    //                VariationOptionId = variationOptionId
    //            };

    //            await _unitOfWork.GetRepository<ProductConfiguration>().InsertAsync(productConfiguration);
    //        }

    //        await _unitOfWork.SaveAsync();
    //    }
    //}

    // Method to generate all combinations of VariationOptionIds
    private List<VariationCombinationDto> GetVariationOptionCombinations(Dictionary<Guid, List<Guid>> variationOptionIdsPerVariation)
    {
        // Generate all possible combinations of variation options based on the IDs per variation
        var allCombinations = new List<VariationCombinationDto>();

        var lists = variationOptionIdsPerVariation.Values.ToList();

        var combinations = GetCombinations(lists);

        foreach (var combination in combinations)
        {
            allCombinations.Add(new VariationCombinationDto
            {
                VariationOptionIds = combination
            });
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