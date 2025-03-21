using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Variation
{
    public class VariationForProductCreationDto
    {
        [Required(ErrorMessage = "Variation ID is required.")]
        public required Guid Id { get; set; }

        [MinLength(1, ErrorMessage = "At least one variation option ID is required.")]
        public List<Guid> VariationOptionIds { get; set; } = new();
    }
}
