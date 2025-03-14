using AutoMapper;
using BusinessObjects.Dto.Brand;
using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.Blog;
using BusinessObjects.Dto.CancelReason;
using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.ProductConfiguration;
using BusinessObjects.Dto.ProductItem;
using BusinessObjects.Dto.ProductStatus;
using BusinessObjects.Dto.Promotion;
using BusinessObjects.Dto.Review;
using BusinessObjects.Dto.Reply;
using BusinessObjects.Models;
using BusinessObjects.Dto.CartItem;
using BusinessObjects.Dto.Variation;
using BusinessObjects.Dto.PromotionType;
using BusinessObjects.Dto.PaymentMethod;
using BusinessObjects.Dto.Role;
using BusinessObjects.Dto.User;
using BusinessObjects.Dto.VariationOption;
using BusinessObjects.Dto.SkinType;
using BusinessObjects.Dto.Order;
using BusinessObjects.Dto.OrderDetail;
using BusinessObjects.Dto.StatusChange;
using BusinessObjects.Dto.ProductForSkinType;
using BusinessObjects.Dto.QuizSet;
using BusinessObjects.Dto.QuizResult;
using BusinessObjects.Dto.Country;

namespace API.Extensions;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region Order
        // Mapping Order -> OrderDto
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.OrderTotal, opt => opt.MapFrom(src => src.OrderTotal))
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime))
            .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src =>
                src.OrderDetails
                    .Select(od => new OrderDetailDto
                    {
                        ProductId = od.ProductItemId,
                        ProductImage = od.ProductItem.Product.ProductImages
                            .Where(pi => pi.IsThumbnail)  // Filter for thumbnails
                            .Select(pi => pi.ImageUrl)    // Select image URL
                            .FirstOrDefault(),            // Get the first thumbnail or null
                        ProductName = od.ProductItem.Product.Name,
                        VariationOptionValues = od.ProductItem.ProductConfigurations
                            .Select(pc => pc.VariationOption.Value)
                            .ToList(),
                        Quantity = od.Quantity,
                        Price = od.Price
                    }).ToList()));  // Mapping to the first OrderDetail (if applicable)

        CreateMap<Order, OrderWithDetailDto>()
            .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src =>
                src.OrderDetails.Select(od => new OrderDetailDto
                {
                    ProductId = od.ProductItemId,
                    ProductImage = od.ProductItem.Product.ProductImages
                        .Where(pi => pi.IsThumbnail)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefault(),
                    ProductName = od.ProductItem.Product.Name,
                    VariationOptionValues = od.ProductItem.ProductConfigurations
                        .Select(pc => pc.VariationOption.Value)
                        .ToList(),
                    Quantity = od.Quantity,
                    Price = od.Price,
                }).ToList()))
            .ForMember(dest => dest.StatusChanges, opt => opt.MapFrom(src =>
                src.StatusChanges.OrderBy(sc => sc.Date)
                    .Select(sc => new StatusChangeDto
                    {
                        Status = sc.Status,
                        Date = sc.Date,
                    }).ToList()));


        CreateMap<OrderForCreationDto, Order>()
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastUpdatedTime, opt => opt.MapFrom(src => DateTime.UtcNow));
        CreateMap<OrderForUpdateDto, Order>();
        #endregion

        #region OrderDetail
        // Mapping OrderDetail -> OrderDetailDto
        CreateMap<OrderDetail, OrderDetailDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductItem.ProductId))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.ProductItem.Product.ProductImages.FirstOrDefault().ImageUrl))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductItem.Product.Name))
            .ForMember(dest => dest.VariationOptionValues, opt => opt.MapFrom(src => src.ProductItem.ProductConfigurations.Select(pc => pc.VariationOption.Value)))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));
        // Map OrderDetail to OrderDetail (for adding OrderDetails to the Order)
        CreateMap<OrderDetailForCreationDto, OrderDetail>()
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastUpdatedTime, opt => opt.MapFrom(src => DateTime.UtcNow));
        #endregion

        #region Product
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Thumbnail,
                opt => opt.MapFrom(src => src.ProductImages.FirstOrDefault(img => img.IsThumbnail).ImageUrl));
        // Mapping từ ProductForCreationDto sang Product
        CreateMap<ProductForCreationDto, Product>()
            .ForMember(dest => dest.ProductItems, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id sẽ được tự động tạo
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.MarketPrice, opt => opt.MapFrom(src => src.MarketPrice))
            .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.BrandId))
            .ForMember(dest => dest.ProductCategoryId, opt => opt.MapFrom(src => src.ProductCategoryId))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            // Mapping Specifications
            .ForMember(dest => dest.StorageInstruction, opt => opt.MapFrom(src => src.Specifications.StorageInstruction))
            .ForMember(dest => dest.UsageInstruction, opt => opt.MapFrom(src => src.Specifications.UsageInstruction))
            .ForMember(dest => dest.DetailedIngredients, opt => opt.MapFrom(src => src.Specifications.DetailedIngredients))
            .ForMember(dest => dest.MainFunction, opt => opt.MapFrom(src => src.Specifications.MainFunction))
            .ForMember(dest => dest.Texture, opt => opt.MapFrom(src => src.Specifications.Texture))
            .ForMember(dest => dest.KeyActiveIngredients, opt => opt.MapFrom(src => src.Specifications.KeyActiveIngredients))
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.Specifications.ExpiryDate))
            .ForMember(dest => dest.SkinIssues, opt => opt.MapFrom(src => src.Specifications.SkinIssues));

        CreateMap<ProductForUpdateDto, Product>();
        CreateMap<Product, ProductWithDetailsDto>()
            .ForMember(dest => dest.ProductImageUrls, opt => opt.MapFrom(src => src.ProductImages.Select(i => i.ImageUrl).ToList()))
            .ForMember(dest => dest.ProductItems, opt => opt.MapFrom(src => src.ProductItems))
            .ForMember(dest => dest.Promotion, opt => opt.MapFrom(src => src.PromotionTargets
                .Where(pt => pt.Promotion != null)
                .Select(pt => new PromotionForProductQueryDto
                {
                    Id = pt.Promotion.Id,
                    Name = pt.Promotion.Name,
                    Type = pt.Promotion.Type,
                    Description = pt.Promotion.Description,
                    DiscountRate = pt.Promotion.DiscountRate,
                    StartDate = pt.Promotion.StartDate,
                    EndDate = pt.Promotion.EndDate
                })
                .FirstOrDefault())) // Chỉ lấy promotion đầu tiên (nếu cần nhiều hơn, chỉnh lại kiểu trả về)
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.ProductCategory))
            .ForMember(dest => dest.Specifications, opt => opt.MapFrom(src => new ProductSpecifications
            {
                DetailedIngredients = src.DetailedIngredients,
                MainFunction = src.MainFunction,
                Texture = src.Texture,
                EnglishName = src.EnglishName,
                KeyActiveIngredients = src.KeyActiveIngredients,
                StorageInstruction = src.StorageInstruction,
                UsageInstruction = src.UsageInstruction,
                ExpiryDate = src.ExpiryDate,
                SkinIssues = src.SkinIssues
            }))
            .ForMember(dest => dest.SkinTypes, opt => opt.MapFrom(src => src.ProductForSkinTypes
                .Where(pst => pst.SkinType != null)
                .Select(pst => new SkinTypeForProductQueryDto
                {
                    Id = pst.SkinType.Id,
                    Name = pst.SkinType.Name
                })
                .ToList()));
        // Mapping from VariationCombinationDto to ProductItem
        CreateMap<VariationCombinationDto, ProductItem>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.QuantityInStock, opt => opt.MapFrom(src => src.QuantityInStock))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));
        #endregion

        #region CancelReason
        CreateMap<CancelReason, CancelReasonDto>();
        CreateMap<CancelReasonForCreationDto, CancelReason>();
        CreateMap<CancelReasonForUpdateDto, CancelReason>();
        #endregion

        #region ProductStatus
        CreateMap<ProductStatus, ProductStatusDto>();
        CreateMap<ProductStatusForCreationDto, ProductStatus>();
        CreateMap<ProductStatusForUpdateDto, ProductStatus>();
        #endregion

        #region ProductItem
        CreateMap<ProductItem, ProductItemDto>()
            .ForMember(dest => dest.Configurations, opt => opt.MapFrom(src => src.ProductConfigurations.Select(config => new ProductConfigurationForProductQueryDto
            {
                VariationName = config.VariationOption.Variation.Name,
                OptionName = config.VariationOption.Value,
                OptionId = config.VariationOption.Id
            }).ToList()));
        #endregion

        #region StatusChange
        // Map StatusChangeForCreationDto to StatusChange (for tracking status change)
        CreateMap<StatusChangeForCreationDto, StatusChange>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateTimeOffset.UtcNow));
        #endregion

        #region Promotion
        CreateMap<Promotion, PromotionForProductQueryDto>();
        #endregion

        #region Brand
        CreateMap<Brand, BrandDto>();
        #endregion

        #region ProductCategory
        CreateMap<ProductCategory, ProductCategoryDto>();
        CreateMap<ProductCategoryForCreationDto, ProductCategory>();
        CreateMap<ProductCategoryForUpdateDto, ProductCategory>();
        #endregion

        #region ProductConfiguration
        CreateMap<ProductConfiguration, ProductConfigurationForProductQueryDto>();
        #endregion

        #region Address
        // Map Address to AddressDto
        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.LastName + " " + src.User.SurName))
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country.CountryName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.StreetNumber, opt => opt.MapFrom(src => src.StreetNumber))
            .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.AddressLine1))
            .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.AddressLine2))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.Ward, opt => opt.MapFrom(src => src.Ward))
            .ForMember(dest => dest.Postcode, opt => opt.MapFrom(src => src.Postcode))
            .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province));
        CreateMap<AddressForCreationDto, Address>();
        CreateMap<AddressForUpdateDto, Address>();
        #endregion

        #region Review
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName)) // Lấy tên người dùng
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.User.AvatarUrl)) // Lấy Avatar URL
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductItem.Product.Name)) // Lấy tên sản phẩm
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src =>
                        src.ProductItem.Product.ProductImages.Any(img => img.IsThumbnail)
                            ? src.ProductItem.Product.ProductImages.FirstOrDefault(img => img.IsThumbnail).ImageUrl
                            : null)) // Check for IsThumbnail without null-propagating // Lấy URL hình ảnh sản phẩm đầu tiên
            .ForMember(dest => dest.ReviewImages, opt => opt.MapFrom(src =>
                src.ReviewImages != null && src.ReviewImages.Any()
                    ? src.ReviewImages.Select(ri => ri.ImageUrl).ToList()
                    : new List<string>())) // Lấy danh sách URL của ReviewImages
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductItem.ProductId)) // Ánh xạ ProductId
            .ForMember(dest => dest.VariationOptionValues, opt => opt.MapFrom(src =>
                src.ProductItem.ProductConfigurations
                    .Select(pc => pc.VariationOption.Value).ToList())) // Lấy danh sách giá trị của VariationOption
            .ForMember(dest => dest.RatingValue, opt => opt.MapFrom(src => src.RatingValue)) // Ánh xạ RatingValue
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment)) // Ánh xạ Comment
            .ForMember(dest => dest.LastUpdatedTime, opt => opt.MapFrom(src => src.LastUpdatedTime)) // Ánh xạ thời gian cập nhật cuối
            .ForMember(dest => dest.Reply, opt => opt.MapFrom(src => src.Reply != null
                ? new ReplyDto
                {
                    Id = src.Reply.Id,
                    UserName = src.Reply.User.UserName,
                    ReplyContent = src.Reply.ReplyContent,
                    LastUpdatedTime = src.Reply.LastUpdatedTime
                }
                : null)); // Ánh xạ Reply nếu tồn tại
        CreateMap<Review, ReviewForProductQueryDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.User.AvatarUrl))
            .ForMember(dest => dest.ReviewImages, opt => opt.MapFrom(src => src.ReviewImages.Select(ri => ri.ImageUrl)))
            .ForMember(dest => dest.VariationOptionValues, opt => opt.MapFrom(src =>
                src.ProductItem.ProductConfigurations.Select(pc => pc.VariationOption.Value)))
            .ForMember(dest => dest.LastUpdatedTime, opt => opt.MapFrom(src => src.LastUpdatedTime))
            .ForMember(dest => dest.Reply, opt => opt.MapFrom(src => src.Reply)); CreateMap<ReviewForCreationDto, Review>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.LastUpdatedTime, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewImages, opt => opt.MapFrom(src =>
                src.ReviewImages != null
                    ? src.ReviewImages.Select(imageUrl => new ReviewImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = imageUrl
                    }).ToList()
                    : new List<ReviewImage>()))
        .ReverseMap(); // Enable reverse mapping

        CreateMap<Review, ReviewForCreationDto>()
            .ForMember(dest => dest.ProductItemId, opt => opt.MapFrom(src => src.ProductItemId))
            .ForMember(dest => dest.ReviewImages, opt => opt.MapFrom(src => src.ReviewImages.Select(ri => ri.ImageUrl).ToList()))
            .ForMember(dest => dest.RatingValue, opt => opt.MapFrom(src => src.RatingValue))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment));

        CreateMap<ReviewForUpdateDto, Review>()
            .ForMember(dest => dest.ReviewImages, opt => opt.Ignore());
        #endregion

        #region Reply
        CreateMap<Reply, ReplyDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));
        CreateMap<ReplyForCreationDto, Reply>();
        CreateMap<ReplyForUpdateDto, Reply>();
        #endregion

        #region CartItem
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.ProductItem.Price))
            .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.ProductItem.QuantityInStock))
            .ForMember(dest => dest.MarketPrice, opt => opt.MapFrom(src => src.ProductItem.Price))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductItem.Product.Id))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductItem.Product.Name))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src =>
                src.ProductItem.Product.ProductImages
                    .Where(pi => pi.IsThumbnail)
                    .Select(pi => pi.ImageUrl)
                    .FirstOrDefault() ?? string.Empty))
            .ForMember(dest => dest.VariationOptionValues, opt => opt.MapFrom(src =>
                src.ProductItem.ProductConfigurations
                    .Where(pc => pc.VariationOption != null)
                    .Select(pc => pc.VariationOption.Value)
                    .ToList()))
            .ForMember(dest => dest.InStock, opt => opt.MapFrom(src =>
                !(src.Quantity > src.ProductItem.QuantityInStock)))
            .ForMember(dest => dest.LastUpdatedTime, opt => opt.MapFrom(src => src.LastUpdatedTime));
        CreateMap<CartItemForCreationDto, CartItem>();
        CreateMap<CartItemForUpdateDto, CartItem>();
        #endregion

        #region PromotionType
        CreateMap<PromotionType, PromotionTypeDto>();
        CreateMap<PromotionTypeForCreationDto, PromotionType>();
        CreateMap<PromotionTypeForUpdateDto, PromotionType>();
        #endregion

        #region PaymentMethod
        CreateMap<PaymentMethod, PaymentMethodDto>();
        CreateMap<PaymentMethodForCreationDto, PaymentMethod>();
        CreateMap<PaymentMethodForUpdateDto, PaymentMethod>();
        #endregion

        #region Variation
        CreateMap<Variation, VariationDto>();
        CreateMap<VariationForCreationDto, Variation>();
        CreateMap<VariationForUpdateDto, Variation>();
        #endregion

        #region VariationOption
        CreateMap<VariationOption, VariationOptionDto>();
        CreateMap<VariationOptionForCreationDto, VariationOption>();
        CreateMap<VariationOptionForUpdateDto, VariationOption>();
        #endregion

        #region PaymentMethod
        CreateMap<PaymentMethod, PaymentMethodDto>();
        CreateMap<PaymentMethodForCreationDto, PaymentMethod>();
        CreateMap<PaymentMethodForUpdateDto, PaymentMethod>();
        #endregion
        
        #region User
        CreateMap<User, UserDto>();
        CreateMap<UserForCreationDto, User>();
        CreateMap<UserForUpdateDto, User>();

        #region Blog

        CreateMap<Blog, BlogDto>();
        CreateMap<Blog, BlogWithDetailDto>();
        CreateMap<BlogForCreationDto, BlogDto>();
        CreateMap<BlogForUpdateDto, Blog>();

        #endregion



        #endregion

        #region Role
        CreateMap<Role, RoleDto>();
        CreateMap<RoleForCreationDto, RoleDto>();
        CreateMap<RoleForUpdateDto, RoleDto>();
        #endregion

        #region ProductForSkinType
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Thumbnail,
                opt => opt.MapFrom(src => src.ProductImages.FirstOrDefault(img => img.IsThumbnail).ImageUrl));
        // Map ProductForSkinType -> ProductForSkinTypeDto (chỉ lấy SkinTypeId, Product sẽ xử lý riêng)
        CreateMap<ProductForSkinType, ProductForSkinTypeDto>()
            .ForMember(dest => dest.SkinTypeId, opt => opt.MapFrom(src => src.SkinTypeId))
            .ForMember(dest => dest.Products, opt => opt.Ignore());
        #endregion

        #region QuizSet
        CreateMap<QuizSet, QuizSetDto>();
        #endregion

        #region QuizResult
        CreateMap<QuizResult, QuizResultDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.SkinType.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.SkinType.Description))
            .ForMember(dest => dest.Routine, opt => opt.MapFrom(src => src.SkinType.Routine));
        #endregion

        #region SkinType
        CreateMap<SkinType, SkinTypeDto>();
        CreateMap<SkinTypeForCreationDto, SkinTypeDto>();
        CreateMap<SkinTypeForUpdateDto, SkinTypeDto>();
        #endregion

        #region Country
        CreateMap<Country, CountryDto>();
        CreateMap<CountryForCreationDto, CountryDto>();
        CreateMap<CountryForUpdateDto, CountryDto>();
        #endregion


    }
}