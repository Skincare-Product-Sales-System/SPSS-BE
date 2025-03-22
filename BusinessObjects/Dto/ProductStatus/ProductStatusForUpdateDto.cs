using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.ProductStatus
{
    public class ProductStatusForUpdateDto
    {
        [Required(ErrorMessage = "Status name is required.")]
        [StringLength(100, ErrorMessage = "Status name can't exceed 100 characters.")]
        public string StatusName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description can't exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
