using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Role;

public class RoleForCreationDto
{
    [Required(ErrorMessage = "Role name is required.")]
    [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters.")]
    public string RoleName { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string Description { get; set; }
}