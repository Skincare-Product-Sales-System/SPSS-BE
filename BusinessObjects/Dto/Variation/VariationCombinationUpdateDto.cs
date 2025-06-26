using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Variation
{
    public class VariationCombinationUpdateDto
    {
        [MinLength(1, ErrorMessage = "At least one variation option ID is required.")]
        public List<Guid>? VariationOptionIds { get; set; }

        [Url(ErrorMessage = "Invalid URL format for Image URL.")]
        [Required(ErrorMessage = "Image URL is required.")]
        public string ImageUrl { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "Price must be a non-negative integer.")]
        public int? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Purchase price must be a non-negative integer.")]
        public int? PurchasePrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity in stock must be a non-negative integer.")]
        public int? QuantityInStock { get; set; }
    }
}
