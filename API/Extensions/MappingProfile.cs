using AutoMapper;
using BusinessObjects.Dto.Brand;
using BusinessObjects.Dto.CancelReason;
using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.ProductConfiguration;
using BusinessObjects.Dto.ProductItem;
using BusinessObjects.Dto.Promotion;
using BusinessObjects.Dto.Review;
using BusinessObjects.Dto.Reply;
using BusinessObjects.Models;
using BusinessObjects.Dto.CartItem;

namespace API.Extensions;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region Product
        CreateMap<Product, ProductDto>();
        CreateMap<ProductForCreationDto, Product>();
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
                VolumeWeight = src.VolumeWeight,
                DetailedIngredients = src.DetailedIngredients,
                RegisterNumber = src.RegisterNumber,
                MainFunction = src.MainFunction,
                Texture = src.Texture,
                EnglishName = src.EnglishName,
                KeyActiveIngredients = src.KeyActiveIngredients,
                StorageInstruction = src.StorageInstruction,
                UsageInstruction = src.UsageInstruction,
                ExpiryDate = src.ExpiryDate,
                SkinIssues = src.SkinIssues
            }));

        #endregion

        #region CancelReason
        CreateMap<CancelReason, CancelReasonDto>();
        CreateMap<CancelReasonForCreationDto, CancelReason>();
        CreateMap<CancelReasonForUpdateDto, CancelReason>();
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

        #region Review
        CreateMap<Review, ReviewDto>();
        CreateMap<ReviewForCreationDto, Review>();
        CreateMap<ReviewForUpdateDto, Review>();
        #endregion

        #region Reply
        CreateMap<Reply, ReplyDto>();
        CreateMap<ReplyForCreationDto, Reply>();
        CreateMap<ReplyForUpdateDto, Reply>();
        #endregion

        #region CartItem
        CreateMap<CartItem, CartItemDto>();
        CreateMap<CartItemForCreationDto, CartItem>();
        CreateMap<CartItemForUpdateDto, CartItem>();
        #endregion
    }
}