using BusinessObjects.Dto.Brand;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.ProductItem;
using BusinessObjects.Dto.SkinType;
using BusinessObjects.Dto.Variation;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Product
{
    public class ProductForEditDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal MarketPrice { get; set; }
        public string Status { get; set; }
        public Guid? BrandId { get; set; }
        public Guid? ProductCategoryId { get; set; }
        public List<string> ProductImageUrls { get; set; } = new List<string>();
        public List<Guid> SkinTypeIds { get; set; } = new List<Guid>();
        public List<VariationForProductEditDto> Variations { get; set; } = new List<VariationForProductEditDto>();
        public List<VariationCombinationEditDto> ProductItems { get; set; } = new List<VariationCombinationEditDto>();
        public ProductSpecifications Specifications { get; set; } = new ProductSpecifications();
    }

    public class VariationForProductEditDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<VariationOptionForEditDto> Options { get; set; } = new List<VariationOptionForEditDto>();
    }

    public class VariationOptionForEditDto
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public bool IsSelected { get; set; }
    }

    public class VariationCombinationEditDto
    {
        public Guid Id { get; set; }
        public List<Guid> VariationOptionIds { get; set; } = new List<Guid>();
        public decimal Price { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public int QuantityInStock { get; set; }
        public string ImageUrl { get; set; }
    }
}