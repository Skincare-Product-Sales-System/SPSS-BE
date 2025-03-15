namespace BusinessObjects.Dto.Variation
{
    public class VariationCombinationUpdateDto
    {
        public List<Guid>? VariationOptionIds { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int? Price { get; set; }
        public int? QuantityInStock { get; set; }
    }
}
