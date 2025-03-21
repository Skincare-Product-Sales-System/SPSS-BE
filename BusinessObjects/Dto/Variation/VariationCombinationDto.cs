using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Variation
{
    public class VariationCombinationDto
    {
        [MinLength(1, ErrorMessage = "At least one variation option ID is required.")]
        public List<Guid> VariationOptionIds { get; set; } = new List<Guid>();

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Price must be a non-negative integer.")]
        public int Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Market price must be a non-negative integer.")]
        public int MarketPrice { get; set; }

        [Required(ErrorMessage = "Quantity in stock is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity in stock must be a non-negative integer.")]
        public int QuantityInStock { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [Url(ErrorMessage = "Invalid URL format for Image URL.")]
        public string ImageUrl { get; set; } = null!;
    }
}
