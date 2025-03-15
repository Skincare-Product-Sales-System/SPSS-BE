namespace BusinessObjects.Dto.Product
{
    public class ProductForQuizResultDto
    {
        public Guid Id { get; set; }

        public string Thumbnail { get; set; } = null!;
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }
        public int SoldCount { get; set; }
    }
}
