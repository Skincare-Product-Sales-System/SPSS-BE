namespace BusinessObjects.Dto.Variation
{
    public class VariationForProductCreationDto
    {
        public required Guid Id { get; set; }
        public List<Guid> VariationOptionIds { get; set; } = [];
    }
}
