using BusinessObjects.Dto.ProductItem;
using BusinessObjects.Dto.Variation;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Product;

public class ProductForUpdateDto
{
    [Required(ErrorMessage = "Brand is required.")]
    public Guid? BrandId { get; set; }
    [Required(ErrorMessage = "Category is required.")]
    public Guid? ProductCategoryId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name can't exceed 200 characters.")]
    public string Name { get; set; }

    [StringLength(1000, ErrorMessage = "Description can't exceed 1000 characters.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Market price must be greater than 0.")]
    public decimal MarketPrice { get; set; }

    public List<Guid> SkinTypeIds { get; set; } = new List<Guid>();

    public List<string> ProductImageUrls { get; set; } = new List<string>();

    public List<VariationForProductUpdateDto>? Variations { get; set; }

    public List<VariationCombinationUpdateDto>? VariationCombinations { get; set; }
    [Required(ErrorMessage = "Specifications are required.")]
    public ProductSpecifications Specifications { get; set; } = new();
}