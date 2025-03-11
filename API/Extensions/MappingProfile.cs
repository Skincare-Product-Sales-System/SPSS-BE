using AutoMapper;
using BusinessObjects.Dto.Brand;
using BusinessObjects.Dto.CancelReason;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductConfiguration;
using BusinessObjects.Dto.ProductItem;
using BusinessObjects.Dto.ProductStatus;
using BusinessObjects.Dto.Promotion;
using BusinessObjects.Models;
using BusinessObjects.Dto.Variation;

namespace API.Extensions;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region Product
        CreateMap<Product, ProductDto>();
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
            .ForMember(dest => dest.VolumeWeight, opt => opt.MapFrom(src => src.Specifications.VolumeWeight))
            .ForMember(dest => dest.DetailedIngredients, opt => opt.MapFrom(src => src.Specifications.DetailedIngredients))
            .ForMember(dest => dest.RegisterNumber, opt => opt.MapFrom(src => src.Specifications.RegisterNumber))
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
    }
}