namespace BusinessObjects.Dto.Variation
{
    public class VariationForProductUpdateDto
    {
        public Guid? Id { get; set; }
        public List<Guid>? VariationOptionIds { get; set; }
    }
}
