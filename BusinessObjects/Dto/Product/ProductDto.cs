namespace BusinessObjects.Dto.Product;

public class ProductDto
{
    public Guid Id { get; set; }

    public string Thumbnail { get; set; } = null!;
    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public decimal MarketPrice { get; set; }

    public int SoldCount { get; set; }
}