using AutoMapper;
using BusinessObjects.Dto.Brand;
using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.ProductConfiguration;
using BusinessObjects.Dto.ProductItem;
using BusinessObjects.Dto.SkinType;
using BusinessObjects.Dto.Variation;
using BusinessObjects.Models;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Repositories.Implementation;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using Shared.Constants;
using System.Linq;

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

    public async Task<PagedResponse<ProductDto>> GetPagedByBrandAsync(Guid brandId, int pageNumber, int pageSize)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            pageNumber,
            pageSize,
            cr => cr.IsDeleted == false && cr.BrandId == brandId
        );

        var orderedProducts = products.OrderByDescending(p => p.CreatedTime).ToList();

        // Update products with minimum prices
        await UpdateProductsWithMinimumPricesAsync(orderedProducts);

        var productIds = orderedProducts.Select(p => p.Id).ToList();
        var productImages = await _unitOfWork.ProductImages.Entities
            .Where(pi => productIds.Contains(pi.ProductId))
            .ToListAsync();

        foreach (var product in orderedProducts)
        {
            product.ProductImages = productImages
                .Where(pi => pi.ProductId == product.Id)
                .ToList();
        }

        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(orderedProducts);

        return new PagedResponse<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResponse<ProductDto>> GetPagedBySkinTypeAsync(Guid skinTypeId, int pageNumber, int pageSize)
    {
        // Fetch products related to the given SkinTypeId via the join table
        var productIds = await _unitOfWork.ProductForSkinTypes.Entities
            .Where(pst => pst.SkinTypeId == skinTypeId)
            .Select(pst => pst.ProductId)
            .Distinct()
            .ToListAsync();

        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            pageNumber,
            pageSize,
            cr => cr.IsDeleted == false && productIds.Contains(cr.Id)
        );

        var orderedProducts = products.OrderByDescending(p => p.CreatedTime).ToList();

        // Update products with minimum prices
        await UpdateProductsWithMinimumPricesAsync(orderedProducts);

        var productImageIds = orderedProducts.Select(p => p.Id).ToList();
        var productImages = await _unitOfWork.ProductImages.Entities
            .Where(pi => productImageIds.Contains(pi.ProductId))
            .ToListAsync();

        foreach (var product in orderedProducts)
        {
            product.ProductImages = productImages
                .Where(pi => pi.ProductId == product.Id)
                .ToList();
        }

        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(orderedProducts);

        return new PagedResponse<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResponse<ProductDto>> GetPagedBySkinTypeAndCategoryAsync(Guid skinTypeId, Guid categoryId, int pageNumber, int pageSize)
    {
        // Fetch product IDs related to the given SkinTypeId via the join table
        var productIdsBySkinType = await _unitOfWork.ProductForSkinTypes.Entities
            .Where(pst => pst.SkinTypeId == skinTypeId)
            .Select(pst => pst.ProductId)
            .Distinct()
            .ToListAsync();

        // Fetch products filtered by both SkinTypeId and CategoryId
        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            pageNumber,
            pageSize,
            cr => cr.IsDeleted == false &&
                  productIdsBySkinType.Contains(cr.Id) &&
                  cr.ProductCategoryId == categoryId
        );

        var orderedProducts = products.OrderByDescending(p => p.CreatedTime).ToList();

        // Update products with minimum prices
        await UpdateProductsWithMinimumPricesAsync(orderedProducts);

        // Fetch related product images
        var productImageIds = orderedProducts.Select(p => p.Id).ToList();
        var productImages = await _unitOfWork.ProductImages.Entities
            .Where(pi => productImageIds.Contains(pi.ProductId))
            .ToListAsync();

        // Assign images to each product
        foreach (var product in orderedProducts)
        {
            product.ProductImages = productImages
                .Where(pi => pi.ProductId == product.Id)
                .ToList();
        }

        // Map products to DTOs
        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(orderedProducts);

        // Return paged response
        return new PagedResponse<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }


    public async Task<ProductWithDetailsDto> GetByIdAsync(Guid id)
    {
        // Lấy sản phẩm từ database
        var product = await _unitOfWork.Products
            .GetQueryable()
            .Include(p => p.ProductCategory)
            .Include(p => p.ProductItems)
                .ThenInclude(pi => pi.ProductConfigurations)
                    .ThenInclude(pc => pc.VariationOption)
                        .ThenInclude(vo => vo.Variation)
            .Include(p => p.Brand)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductForSkinTypes)
                .ThenInclude(pst => pst.SkinType)
            .Include(ps => ps.ProductStatus)
            .FirstOrDefaultAsync(p => p.Id == id);

        // Kiểm tra null
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found.");

        // Thủ công map dữ liệu từ entity sang DTO
        var productDto = new ProductWithDetailsDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            MarketPrice = product.MarketPrice,
            Rating = product.Rating,
            SoldCount = product.SoldCount,
            Status = product.ProductStatus.StatusName,
            Thumbnail = product.ProductImages.FirstOrDefault(pi => pi.IsThumbnail)?.ImageUrl,
            Category = new ProductCategoryDto
            {
                Id = product.ProductCategory.Id,
                CategoryName = product.ProductCategory.CategoryName
            },
            Brand = new BrandDto
            {
                Id = product.Brand.Id,
                Name = product.Brand.Name,
                Title = product.Brand.Title,
                Description = product.Brand.Description,
                ImageUrl = product.Brand.ImageUrl
            },
            ProductItems = product.ProductItems.Select(pi => new ProductItemDto
            {
                Id = pi.Id,
                Price = pi.Price,
                MarketPrice = pi.MarketPrice,
                PurchasePrice = pi.PurchasePrice,
                QuantityInStock = pi.QuantityInStock,
                ImageUrl = pi.ImageUrl,
                Configurations = pi.ProductConfigurations.Select(pc => new ProductConfigurationForProductQueryDto
                {
                    VariationName = pc.VariationOption.Variation.Name,
                    OptionName = pc.VariationOption.Value,
                    OptionId = pc.VariationOption.Id
                }).ToList()
            }).ToList(),
            SkinTypes = product.ProductForSkinTypes.Select(pst => new SkinTypeForProductQueryDto
            {
                Id = pst.SkinType.Id,
                Name = pst.SkinType.Name
            }).ToList(),
            Specifications = new ProductSpecifications
            {
                StorageInstruction = product.StorageInstruction,
                UsageInstruction = product.UsageInstruction,
                DetailedIngredients = product.DetailedIngredients,
                MainFunction = product.MainFunction,
                Texture = product.Texture,
                KeyActiveIngredients = product.KeyActiveIngredients,
                ExpiryDate = product.ExpiryDate,
                SkinIssues = product.SkinIssues
            }
        };

        return productDto;
    }

    public async Task<PagedResponse<ProductDto>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    Guid? brandId = null,
    Guid? categoryId = null,
    Guid? skinTypeId = null,
    string sortBy = "newest",
    string name = null)
    {
        // Existing code for filtering products
        var productIdsBySkinType = skinTypeId.HasValue
            ? await _unitOfWork.ProductForSkinTypes.Entities
                .Where(pst => pst.SkinTypeId == skinTypeId.Value)
                .Select(pst => pst.ProductId)
                .Distinct()
                .ToListAsync()
            : null;

        var subCategoryIds = categoryId.HasValue
            ? await GetSubCategoryIdsAsync(categoryId.Value)
            : null;

        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            pageNumber,
            pageSize,
            cr =>
                cr.IsDeleted == false &&
                (!brandId.HasValue || cr.BrandId == brandId.Value) &&
                (subCategoryIds == null || subCategoryIds.Contains(cr.ProductCategoryId)) &&
                (productIdsBySkinType == null || productIdsBySkinType.Contains(cr.Id)) &&
                (string.IsNullOrEmpty(name) || cr.Name.Contains(name))
        );

        // Update products with minimum prices from their items
        await UpdateProductsWithMinimumPricesAsync(products);

        // Existing code for sorting
        products = sortBy.ToLower() switch
        {
            "newest" => products.OrderByDescending(p => p.CreatedTime),
            "bestselling" => products.OrderByDescending(p => p.SoldCount),
            "price_asc" => products.OrderBy(p => p.Price),
            "price_desc" => products.OrderByDescending(p => p.Price),
            _ => products.OrderByDescending(p => p.CreatedTime)
        };

        // Existing code for loading images
        var productIds = products.Select(p => p.Id).ToList();
        var productImages = await _unitOfWork.ProductImages.Entities
            .Where(pi => productIds.Contains(pi.ProductId))
            .ToListAsync();

        foreach (var product in products)
        {
            product.ProductImages = productImages
                .Where(pi => pi.ProductId == product.Id)
                .ToList();
        }

        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        return new PagedResponse<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    // Add this helper method to the ProductService class
    private async Task UpdateProductsWithMinimumPricesAsync(IEnumerable<Product> products)
    {
        var productIds = products.Select(p => p.Id).ToList();

        // Get all product items for these products
        var productItems = await _unitOfWork.ProductItems.Entities
            .Where(pi => productIds.Contains(pi.ProductId))
            .ToListAsync();

        // Group product items by product id
        var productItemsGrouped = productItems.GroupBy(pi => pi.ProductId)
                                             .ToDictionary(g => g.Key, g => g.ToList());

        // Update each product with minimum prices from its items
        foreach (var product in products)
        {
            if (productItemsGrouped.TryGetValue(product.Id, out var items) && items.Any())
            {
                // Set product price to the minimum price from its items
                var minPriceItem = items.OrderBy(i => i.Price).FirstOrDefault();
                var minMarketPriceItem = items.OrderBy(i => i.MarketPrice).FirstOrDefault();

                if (minPriceItem != null)
                    product.Price = minPriceItem.Price;

                if (minMarketPriceItem != null)
                    product.MarketPrice = minMarketPriceItem.MarketPrice;
            }
        }
    }

    private async Task<List<Guid>> GetSubCategoryIdsAsync(Guid categoryId)
    {
        var allCategories = await _unitOfWork.ProductCategories.Entities.ToListAsync();

        // Sử dụng BFS hoặc DFS để lấy tất cả các CategoryId con
        var subCategoryIds = new List<Guid> { categoryId };
        var queue = new Queue<Guid>();
        queue.Enqueue(categoryId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = allCategories
                .Where(c => c.ParentCategoryId == currentId)
                .Select(c => c.Id)
                .ToList();

            foreach (var childId in children)
            {
                if (!subCategoryIds.Contains(childId))
                {
                    subCategoryIds.Add(childId);
                    queue.Enqueue(childId);
                }
            }
        }

        return subCategoryIds;
    }

    public async Task<PagedResponse<ProductDto>> GetBestSellerAsync(int pageNumber, int pageSize)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            pageNumber,
            pageSize,
            cr => cr.IsDeleted == false
        );

        // Sort by SoldCount in descending order
        var orderedProducts = products.OrderByDescending(p => p.SoldCount).ToList();

        // Update products with minimum prices
        await UpdateProductsWithMinimumPricesAsync(orderedProducts);

        var productIds = orderedProducts.Select(p => p.Id).ToList();
        var productImages = await _unitOfWork.ProductImages.Entities
            .Where(pi => productIds.Contains(pi.ProductId))
            .ToListAsync();

        foreach (var product in orderedProducts)
        {
            product.ProductImages = productImages
                .Where(pi => pi.ProductId == product.Id)
                .ToList();
        }

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

            var brandExists = await _unitOfWork.Brands.Entities
                .AnyAsync(c => c.Id == productDto.BrandId);
            if (!categoryExists)
            {
                throw new ArgumentNullException($"Brand with ID {productDto.BrandId} not found.");
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

            // Kiểm tra và xử lý SkinTypeIds
            foreach (var skinTypeId in productDto.SkinTypeIds)
            {
                var skinTypeExists = await _unitOfWork.SkinTypes.Entities
                    .AnyAsync(st => st.Id == skinTypeId);
                if (!skinTypeExists)
                {
                    throw new ArgumentException($"SkinType with ID {skinTypeId} does not exist.");
                }

                // Thêm bản ghi vào bảng trung gian ProductForSkinType
                productEntity.ProductForSkinTypes.Add(new ProductForSkinType
                {
                    Id = Guid.NewGuid(),
                    ProductId = productEntity.Id,
                    SkinTypeId = skinTypeId,
                });
            }

            productEntity.Id = Guid.NewGuid();
            productEntity.CreatedTime = DateTime.UtcNow;
            productEntity.LastUpdatedTime = DateTime.UtcNow;
            productEntity.SoldCount = 0;
            productEntity.Rating = 0;
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

            // Determine the default thumbnail URL if ImageUrl is empty
            var defaultThumbnail = product.ProductImages
                .FirstOrDefault(image => image.IsThumbnail)?.ImageUrl;

            // Create a new ProductItem
            var productItem = new ProductItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Price = combination.Price,
                MarketPrice = combination.MarketPrice,
                PurchasePrice = combination.PurchasePrice,
                QuantityInStock = combination.QuantityInStock,
                ImageUrl = string.IsNullOrWhiteSpace(combination.ImageUrl)
                ? defaultThumbnail // Use Product.Thumbnail if ImageUrl is empty
                : combination.ImageUrl,
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

   public async Task<bool> UpdateAsync(ProductForUpdateDto productDto, Guid userId, Guid productId)
{
    await _unitOfWork.BeginTransactionAsync();

    try
    {
        // Step 1: Retrieve the existing product
        var existingProduct = await _unitOfWork.Products.Entities
            .Include(p => p.ProductItems)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductForSkinTypes)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (existingProduct == null)
        {
            throw new ArgumentException("Product not found.");
        }

        // Step 2: Update simple fields
        if (!string.IsNullOrEmpty(productDto.Name)) existingProduct.Name = productDto.Name;
        if (!string.IsNullOrEmpty(productDto.Description)) existingProduct.Description = productDto.Description;
        if (productDto.BrandId.HasValue) existingProduct.BrandId = productDto.BrandId.Value;
        if (productDto.ProductCategoryId.HasValue) existingProduct.ProductCategoryId = productDto.ProductCategoryId.Value;
        existingProduct.Price = productDto.Price;
        existingProduct.MarketPrice = productDto.MarketPrice;
        existingProduct.LastUpdatedBy = userId.ToString();
        existingProduct.LastUpdatedTime = DateTime.UtcNow;

        // Step 3: Update SkinTypeIds
        var existingSkinTypeIds = existingProduct.ProductForSkinTypes.Select(pfs => pfs.SkinTypeId).ToList();
        var skinTypesToRemove = existingSkinTypeIds.Except(productDto.SkinTypeIds).ToList();
        var skinTypesToAdd = productDto.SkinTypeIds.Except(existingSkinTypeIds).ToList();

        foreach (var skinTypeId in skinTypesToRemove)
        {
            var toRemove = existingProduct.ProductForSkinTypes.FirstOrDefault(pfs => pfs.SkinTypeId == skinTypeId);
            if (toRemove != null)
                _unitOfWork.ProductForSkinTypes.Delete(toRemove);
        }

        foreach (var skinTypeId in skinTypesToAdd)
        {
            _unitOfWork.ProductForSkinTypes.Add(new ProductForSkinType
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                SkinTypeId = skinTypeId,
            });
        }

        // Step 4: Update ProductImages
        var existingImageUrls = existingProduct.ProductImages.Select(pi => pi.ImageUrl).ToList();
        var imagesToRemove = existingProduct.ProductImages.Where(pi => !productDto.ProductImageUrls.Contains(pi.ImageUrl)).ToList();
        var imagesToAdd = productDto.ProductImageUrls.Except(existingImageUrls).ToList();

        foreach (var image in imagesToRemove)
        {
            _unitOfWork.ProductImages.Delete(image);
        }

        for (int i = 0; i < imagesToAdd.Count; i++)
        {
            _unitOfWork.ProductImages.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ImageUrl = imagesToAdd[i],
                IsThumbnail = (i == 0 && !existingProduct.ProductImages.Any(pi => pi.IsThumbnail && !imagesToRemove.Contains(pi))),
            });
        }

        // Step 5: Update Variations and VariationOptions
        if (productDto.Variations != null)
        {
            foreach (var variationDto in productDto.Variations)
            {
                var variationExists = await _unitOfWork.Variations.Entities
                    .AnyAsync(v => v.Id == variationDto.Id);
                if (!variationExists)
                {
                    throw new ArgumentException("Variation not found.");
                }

                if (variationDto.VariationOptionIds != null)
                {
                    foreach (var variationOptionId in variationDto.VariationOptionIds)
                    {
                        var variationOptionExists = await _unitOfWork.VariationOptions.Entities
                            .AnyAsync(vo => vo.Id == variationOptionId && vo.VariationId == variationDto.Id);
                        if (!variationOptionExists)
                        {
                            throw new ArgumentException("Variation Option Not Belong To Variation.");
                        }
                    }
                }
            }
        }

        // Step 6: Update ProductItems and ProductConfigurations
        if (productDto.VariationCombinations != null)
        {
            await UpdateVariationOptionsForProduct(existingProduct, productDto.VariationCombinations, userId);
        }

        existingProduct.LastUpdatedBy = userId.ToString();
        existingProduct.LastUpdatedTime = DateTime.UtcNow;

        // Step 7: Save Changes
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

    public async Task UpdateVariationOptionsForProduct(Product product, List<VariationCombinationUpdateDto> variationCombinations, Guid userId)
    {
        // Delete all existing ProductItems and ProductConfigurations associated with this product
        var productItems = product.ProductItems.ToList();
        _unitOfWork.ProductItems.RemoveRange(productItems);

        // Insert updated ProductItems and ProductConfigurations
        foreach (var combination in variationCombinations)
        {
            var productItem = new ProductItem
            {
                Id = Guid.NewGuid(),
                ImageUrl = combination.ImageUrl,
                ProductId = product.Id,
                Price = combination.Price ?? 0,  // Defaulting to 0 if Price is not provided
                MarketPrice = combination.Price ?? 0, // Default to same as price if not provided
                PurchasePrice = combination.PurchasePrice ?? 0, // Default to 0 if PurchasePrice is not provided
                QuantityInStock = combination.QuantityInStock ?? 0,
            };

            _unitOfWork.ProductItems.Add(productItem);

            if (combination.VariationOptionIds != null)
            {
                foreach (var variationOptionId in combination.VariationOptionIds)
                {
                    var productConfiguration = new ProductConfiguration
                    {
                        Id = Guid.NewGuid(),
                        ProductItemId = productItem.Id,
                        VariationOptionId = variationOptionId
                    };

                    _unitOfWork.ProductConfigurations.Add(productConfiguration);
                }
            }
        }
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

    public async Task<PagedResponse<ProductDto>> GetByCategoryIdPagedAsync(Guid categoryId, int pageNumber, int pageSize)
    {
        var query = _unitOfWork.Products.GetQueryable()
            .Where(p => p.ProductCategoryId == categoryId && !p.IsDeleted);

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderByDescending(p => p.CreatedTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var productIds = products.Select(p => p.Id).ToList();

        var productImages = await _unitOfWork.ProductImages.Entities
            .Where(pi => productIds.Contains(pi.ProductId))
            .ToListAsync();

        foreach (var product in products)
        {
            product.ProductImages = productImages
                .Where(pi => pi.ProductId == product.Id)
                .ToList();
        }

        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        return new PagedResponse<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductForEditDto> GetProductForEditAsync(Guid id)
    {
        // Lấy sản phẩm từ database với tất cả thông tin liên quan
        var product = await _unitOfWork.Products
            .GetQueryable()
            .Include(p => p.ProductCategory)
            .Include(p => p.ProductItems)
                .ThenInclude(pi => pi.ProductConfigurations)
                    .ThenInclude(pc => pc.VariationOption)
                        .ThenInclude(vo => vo.Variation)
            .Include(p => p.Brand)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductForSkinTypes)
                .ThenInclude(pst => pst.SkinType)
            .Include(ps => ps.ProductStatus)
            .FirstOrDefaultAsync(p => p.Id == id);

        // Kiểm tra null
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found.");

        // Lấy tất cả variations và options cho sản phẩm
        var allVariations = await _unitOfWork.Variations.Entities
            .Include(v => v.VariationOptions)
            .ToListAsync();

        // Tạo ProductForEditDto
        var productForEditDto = new ProductForEditDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            MarketPrice = product.MarketPrice,
            Status = product.ProductStatus?.StatusName,
            BrandId = product.BrandId,
            ProductCategoryId = product.ProductCategoryId,
            ProductImageUrls = product.ProductImages.Select(pi => pi.ImageUrl).ToList(),
            SkinTypeIds = product.ProductForSkinTypes.Select(pst => pst.SkinTypeId).ToList(),
            Specifications = new ProductSpecifications
            {
                StorageInstruction = product.StorageInstruction,
                UsageInstruction = product.UsageInstruction,
                DetailedIngredients = product.DetailedIngredients,
                MainFunction = product.MainFunction,
                Texture = product.Texture,
                KeyActiveIngredients = product.KeyActiveIngredients,
                ExpiryDate = product.ExpiryDate,
                SkinIssues = product.SkinIssues,
                EnglishName = product.EnglishName
            }
        };

        // Tìm tất cả variation options đang được sử dụng trong sản phẩm này
        var usedVariationOptionIds = product.ProductItems
            .SelectMany(pi => pi.ProductConfigurations)
            .Select(pc => pc.VariationOptionId)
            .Distinct()
            .ToHashSet();

        // Map các variations và options
        var variationsForProduct = new List<VariationForProductEditDto>();

        foreach (var variation in allVariations)
        {
            var variationDto = new VariationForProductEditDto
            {
                Id = variation.Id,
                Name = variation.Name,
                Options = variation.VariationOptions.Select(vo => new VariationOptionForEditDto
                {
                    Id = vo.Id,
                    Value = vo.Value,
                    // Đánh dấu là selected nếu option này đang được sử dụng trong sản phẩm
                    IsSelected = usedVariationOptionIds.Contains(vo.Id)
                }).ToList()
            };

            // Chỉ thêm variation có ít nhất 1 option được sử dụng
            if (variationDto.Options.Any(o => o.IsSelected))
            {
                variationsForProduct.Add(variationDto);
            }
        }

        productForEditDto.Variations = variationsForProduct;

        // Map các product items với variation combinations
        productForEditDto.ProductItems = product.ProductItems.Select(pi => new VariationCombinationEditDto
        {
            Id = pi.Id,
            Price = pi.Price,
            MarketPrice = pi.MarketPrice,
            PurchasePrice = pi.PurchasePrice,
            QuantityInStock = pi.QuantityInStock,
            ImageUrl = pi.ImageUrl,
            VariationOptionIds = pi.ProductConfigurations.Select(pc => pc.VariationOptionId).ToList()
        }).ToList();

        return productForEditDto;
    }

    public async Task<bool> UpdateProductAsync(Guid id, ProductForEditDto productDto, string userId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Lấy sản phẩm từ database
            var existingProduct = await _unitOfWork.Products.Entities
                .Include(p => p.ProductItems)
                    .ThenInclude(pi => pi.ProductConfigurations)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductForSkinTypes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            // Cập nhật thông tin cơ bản
            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;
            existingProduct.MarketPrice = productDto.MarketPrice;
            existingProduct.BrandId = productDto.BrandId.Value;
            existingProduct.ProductCategoryId = productDto.ProductCategoryId.Value;
            existingProduct.LastUpdatedBy = userId;
            existingProduct.LastUpdatedTime = DateTime.UtcNow;

            // Cập nhật specifications
            existingProduct.StorageInstruction = productDto.Specifications.StorageInstruction;
            existingProduct.UsageInstruction = productDto.Specifications.UsageInstruction;
            existingProduct.DetailedIngredients = productDto.Specifications.DetailedIngredients;
            existingProduct.MainFunction = productDto.Specifications.MainFunction;
            existingProduct.Texture = productDto.Specifications.Texture;
            existingProduct.KeyActiveIngredients = productDto.Specifications.KeyActiveIngredients;
            existingProduct.ExpiryDate = productDto.Specifications.ExpiryDate;
            existingProduct.SkinIssues = productDto.Specifications.SkinIssues;
            existingProduct.EnglishName = productDto.Specifications.EnglishName;

            // Cập nhật SkinTypes
            var existingSkinTypeIds = existingProduct.ProductForSkinTypes.Select(pfs => pfs.SkinTypeId).ToList();
            var skinTypesToRemove = existingSkinTypeIds.Except(productDto.SkinTypeIds).ToList();
            var skinTypesToAdd = productDto.SkinTypeIds.Except(existingSkinTypeIds).ToList();

            // Xóa các liên kết cũ
            foreach (var skinTypeId in skinTypesToRemove)
            {
                var toRemove = existingProduct.ProductForSkinTypes.FirstOrDefault(pfs => pfs.SkinTypeId == skinTypeId);
                if (toRemove != null)
                    _unitOfWork.ProductForSkinTypes.Delete(toRemove);
            }

            // Thêm các liên kết mới
            foreach (var skinTypeId in skinTypesToAdd)
            {
                _unitOfWork.ProductForSkinTypes.Add(new ProductForSkinType
                {
                    Id = Guid.NewGuid(),
                    ProductId = id,
                    SkinTypeId = skinTypeId,
                });
            }

            // Cập nhật ProductImages
            var existingImageUrls = existingProduct.ProductImages.Select(pi => pi.ImageUrl).ToList();
            var imagesToRemove = existingProduct.ProductImages.Where(pi => !productDto.ProductImageUrls.Contains(pi.ImageUrl)).ToList();
            var imagesToAdd = productDto.ProductImageUrls.Except(existingImageUrls).ToList();

            // Xóa các images cũ
            foreach (var image in imagesToRemove)
            {
                _unitOfWork.ProductImages.Delete(image);
            }

            // Thêm các images mới
            for (int i = 0; i < imagesToAdd.Count; i++)
            {
                _unitOfWork.ProductImages.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = id,
                    ImageUrl = imagesToAdd[i],
                    // Đánh dấu là thumbnail nếu là ảnh đầu tiên và chưa có thumbnail
                    IsThumbnail = (i == 0 && !existingProduct.ProductImages.Any(pi => pi.IsThumbnail && !imagesToRemove.Contains(pi))),
                });
            }

            // Xóa tất cả ProductItems và ProductConfigurations hiện tại
            var productItems = existingProduct.ProductItems.ToList();
            foreach (var item in productItems)
            {
                // Xóa các configurations trước
                var configurations = item.ProductConfigurations.ToList();
                foreach (var config in configurations)
                {
                    _unitOfWork.ProductConfigurations.Delete(config);
                }
                // Sau đó xóa item
                _unitOfWork.ProductItems.Delete(item);
            }

            // Thêm các ProductItems và ProductConfigurations mới
            foreach (var item in productDto.ProductItems)
            {
                var productItem = new ProductItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = id,
                    Price = item.Price,
                    MarketPrice = item.MarketPrice,
                    PurchasePrice = item.PurchasePrice,
                    QuantityInStock = item.QuantityInStock,
                    ImageUrl = item.ImageUrl
                };

                _unitOfWork.ProductItems.Add(productItem);

                // Thêm các ProductConfigurations cho item này
                foreach (var optionId in item.VariationOptionIds)
                {
                    var productConfiguration = new ProductConfiguration
                    {
                        Id = Guid.NewGuid(),
                        ProductItemId = productItem.Id,
                        VariationOptionId = optionId
                    };

                    _unitOfWork.ProductConfigurations.Add(productConfiguration);
                }
            }

            // Lưu thay đổi
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
}