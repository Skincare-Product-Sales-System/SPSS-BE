using BusinessObjects.Dto.ProductConfiguration;

namespace BusinessObjects.Dto.ProductItem
{
    public class ProductItemDto
    {
        public Guid Id { get; set; }

        public int QuantityInStock { get; set; }

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }

        public decimal MarketPrice { get; set; }

        public List<ProductConfigurationForProductQueryDto> Configurations { get; set; } = new();

    }
}
