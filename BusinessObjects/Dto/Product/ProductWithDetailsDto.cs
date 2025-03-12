using BusinessObjects.Dto.Brand;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.ProductItem;
using BusinessObjects.Dto.Promotion;
using BusinessObjects.Dto.SkinType;
using BusinessObjects.Models;

namespace BusinessObjects.Dto.Product
{
    public class ProductWithDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public int SoldCount { get; set; }

        public double Rating { get; set; }

        public decimal Price { get; set; }

        public decimal MarketPrice { get; set; }
        public string Status { get; set; } = null!;

        public List<SkinTypeForProductQueryDto> skinTypes { get; set; } = new();
        public List<string> ProductImageUrls { get; set; } = new();
        public List<ProductItemDto> ProductItems { get; set; } = new();
        public PromotionForProductQueryDto? Promotion { get; set; }
        public BrandDto Brand { get; set; } = null!;
        public ProductCategoryDto Category { get; set; } = null!;
        public ProductSpecifications Specifications { get; set; } = new();
    }
}
