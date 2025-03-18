namespace BusinessObjects.Dto.OrderDetail
{
    public class OrderDetailDto
    {
        public Guid ProductId { get; set; }
        public Guid ProductItemId { get; set; }
        public string ProductImage { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public List<string> VariationOptionValues { get; set; } = new List<string>();
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public bool IsReviewable { get; set; }
    }
}
