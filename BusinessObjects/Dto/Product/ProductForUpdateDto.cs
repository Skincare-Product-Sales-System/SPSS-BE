namespace BusinessObjects.Dto.Product;

public class ProductForUpdateDto
{
    public Guid Id { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal MarketPrice { get; set; }
    
    public ProductSpecifications Specifications { get; set; } = new();
}