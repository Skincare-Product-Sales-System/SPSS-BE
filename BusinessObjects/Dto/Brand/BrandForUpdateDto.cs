using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Brand;

public class BrandForUpdateDto
{
    [Required(ErrorMessage = "Country ID is required.")]
    public int CountryId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name can't exceed 100 characters.")]
    public string Name { get; set; } = null!;

    [StringLength(150, ErrorMessage = "Title can't exceed 150 characters.")]
    public string Title { get; set; }

    [StringLength(500, ErrorMessage = "Description can't exceed 500 characters.")]
    public string Description { get; set; }

    [Url(ErrorMessage = "Invalid URL format for Image URL.")]
    public string ImageUrl { get; set; }

    public bool? IsLiked { get; set; }
}