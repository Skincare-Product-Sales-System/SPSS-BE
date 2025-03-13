using BusinessObjects.Dto.ProductItem;
using BusinessObjects.Dto.Variation;

namespace BusinessObjects.Dto.Product;

public class ProductForUpdateDto
{
    public Guid? BrandId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal MarketPrice { get; set; }
    public List<Guid> SkinTypeIds { get; set; } = new List<Guid>();
    public List<string> ProductImageUrls { get; set; } = new List<string>();
    public List<ProductItemForUpdateDto> ProductItems { get; set; } = new List<ProductItemForUpdateDto>();
    public List<VariationForProductCreationDto> Variations { get; set; } = [];
    public List<VariationCombinationDto> NewProductItems { get; set; } = [];
    public ProductSpecifications Specifications { get; set; } = new();
}