namespace BusinessObjects.Dto.Variation
{
    public class VariationCombinationDto
    {
        public List<Guid> VariationOptionIds { get; set; } = [];
        public int Price { get; set; }
        public int QuantityInStock { get; set; }
    }
}
