using BusinessObjects.Dto.SkincareRoutinStep;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.SkinType;

public class SkinTypeForCreationDto
{
    [Required(ErrorMessage = "Skin type name is required.")]
    [StringLength(100, ErrorMessage = "Skin type name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "At least one routine step is required.")]
    public List<SkinTypeRoutineStepForCreationDto> SkinTypeRoutineSteps { get; set; } = new List<SkinTypeRoutineStepForCreationDto>();
}