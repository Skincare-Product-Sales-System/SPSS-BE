namespace BusinessObjects.Dto.ProductItem
{
    public class ProductItemForUpdateDto
    {
        public Guid Id { get; set; }

        public int QuantityInStock { get; set; }

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }
    }
}
