using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.VariationOption
{

    public class VariationOptionForUpdateDto
    {
        [Required(ErrorMessage = "Value is required.")]
        [StringLength(100, ErrorMessage = "Value cannot exceed 100 characters.")]
        public string Value { get; set; }

        [Required(ErrorMessage = "Variation ID is required.")]
        public Guid VariationId { get; set; }
    }
}
