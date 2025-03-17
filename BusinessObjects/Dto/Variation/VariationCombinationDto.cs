namespace BusinessObjects.Dto.Variation
{
    public class VariationCombinationDto
    {
        public List<Guid> VariationOptionIds { get; set; } = new List<Guid>();
        public int Price { get; set; }
        public int MarketPrice { get; set; }
        public int QuantityInStock { get; set; }
        public string ImageUrl { get; set; } = null!;
    }
}
