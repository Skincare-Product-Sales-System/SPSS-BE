using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.SkinType;

public class SkinTypeForUpdateDto
{
    [Required(ErrorMessage = "Skin type name is required.")]
    [StringLength(100, ErrorMessage = "Skin type name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string Description { get; set; }

    [StringLength(2000, ErrorMessage = "Routine details cannot exceed 2000 characters.")]
    public string Routine { get; set; }
}
