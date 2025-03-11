using BusinessObjects.Dto.Variation;

namespace BusinessObjects.Dto.Product;

public class ProductForCreationDto
{
    public Guid? BrandId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int SoldCount { get; set; }
    public double Rating { get; set; }
    public decimal Price { get; set; }
    public decimal MarketPrice { get; set; }
    public List<VariationForProductCreationDto> Variations { get; set; } = [];
    public List<VariationCombinationDto> VariationCombinations { get; set; } = [];
    public ProductSpecifications Specifications { get; set; } = new();
}