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
            .ForMember(dest => dest.OrderDetail, opt => opt.MapFrom(src => src.OrderDetails.FirstOrDefault()));
        CreateMap<OrderForCreationDto, Order>();
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
            .ForMember(dest => dest.skinTypes, opt => opt.MapFrom(src => src.ProductForSkinTypes
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
        CreateMap<Address, AddressDto>();
        CreateMap<AddressForCreationDto, Address>();
        CreateMap<AddressForUpdateDto, Address>();
        #endregion

        #region Review
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName)) // Lấy tên người dùng
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.User.AvatarUrl)) // Lấy Avatar URL
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src =>
                src.ProductItem.Product.ProductImages != null && src.ProductItem.Product.ProductImages.Any()
                    ? src.ProductItem.Product.ProductImages.FirstOrDefault().ImageUrl
                    : null)) // Lấy URL hình ảnh sản phẩm đầu tiên
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
            .ForMember(dest => dest.Reply, opt => opt.MapFrom(src => src.Reply));
        CreateMap<ReviewForCreationDto, Review>();
        CreateMap<ReviewForUpdateDto, Review>();
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
        CreateMap<BlogForCreationDto, BlogDto>();
        CreateMap<BlogForUpdateDto, Blog>();

        #endregion



        #endregion

        #region Role
        CreateMap<Role, RoleDto>();
        CreateMap<RoleForCreationDto, RoleDto>();
        CreateMap<RoleForUpdateDto, RoleDto>();
        #endregion
    }
}