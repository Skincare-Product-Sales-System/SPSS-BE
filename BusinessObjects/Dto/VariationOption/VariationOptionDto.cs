using BusinessObjects.Dto.Variation;

namespace BusinessObjects.Dto.VariationOption
{
    public class VariationOptionDto
    {
        public Guid Id { get; set; }

        public string Value { get; set; }

        public Guid VariationId { get; set; }
        public VariationDto2 VariationDto2 { get; set; }
    }
}
