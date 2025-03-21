using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.ProductItem
{
    public class ProductItemForUpdateDto
    {
        [Required(ErrorMessage = "Product item ID is required.")]
        public Guid Id { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity in stock must be a non-negative integer.")]
        public int QuantityInStock { get; set; }

        [Url(ErrorMessage = "Invalid URL format for Image URL.")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
    }
}
