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
            ProductImageUrls = product.ProductImages.Select(pi => pi.ImageUrl).ToList(),
            ProductItems = product.ProductItems.Select(pi => new ProductItemDto
            {
                Id = pi.Id,
                Price = pi.Price,
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

    public async Task<PagedResponse<ProductDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            pageNumber,
            pageSize,
            cr => cr.IsDeleted == false
        );

        var orderedProducts = products.OrderByDescending(p => p.CreatedTime).ToList();

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

    public async Task<PagedResponse<ProductDto>> GetBestSellerAsync(int pageNumber, int pageSize)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            pageNumber,
            pageSize,
            cr => cr.IsDeleted == false
        );

        // Sort by SoldCount in descending order
        var orderedProducts = products.OrderByDescending(p => p.SoldCount).ToList();

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

            // Create a new ProductItem
            var productItem = new ProductItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Price = combination.Price,
                QuantityInStock = combination.QuantityInStock,
                ImageUrl = combination.ImageUrl,
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
                    IsThumbnail = (i == 0 && !existingProduct.ProductImages.Any(pi => pi.IsThumbnail)),
                });
            }

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

}