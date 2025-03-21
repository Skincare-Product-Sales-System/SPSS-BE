using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Product
{
    public class ProductSpecifications
    {
        [StringLength(2000, ErrorMessage = "Detailed ingredients can't exceed 2000 characters.")]
        public string? DetailedIngredients { get; set; }

        [StringLength(500, ErrorMessage = "Main function can't exceed 500 characters.")]
        public string? MainFunction { get; set; }

        [StringLength(200, ErrorMessage = "Texture can't exceed 200 characters.")]
        public string? Texture { get; set; }

        [StringLength(200, ErrorMessage = "English name can't exceed 200 characters.")]
        public string? EnglishName { get; set; }

        [StringLength(500, ErrorMessage = "Key active ingredients can't exceed 500 characters.")]
        public string? KeyActiveIngredients { get; set; }

        [StringLength(500, ErrorMessage = "Storage instructions can't exceed 500 characters.")]
        public string? StorageInstruction { get; set; }

        [StringLength(500, ErrorMessage = "Usage instructions can't exceed 500 characters.")]
        public string? UsageInstruction { get; set; }

        [StringLength(50, ErrorMessage = "Expiry date can't exceed 50 characters.")]
        public string? ExpiryDate { get; set; }

        [StringLength(500, ErrorMessage = "Skin issues can't exceed 500 characters.")]
        public string? SkinIssues { get; set; }
    }
}
