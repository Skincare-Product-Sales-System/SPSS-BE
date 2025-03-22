using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.SkincareRoutinStep
{
    public class SkinTypeRoutineStepForCreationDto
    {
        [Required(ErrorMessage = "Step name is required.")]
        [StringLength(100, ErrorMessage = "Step name cannot exceed 100 characters.")]
        public string StepName { get; set; }

        [StringLength(500, ErrorMessage = "Instruction cannot exceed 500 characters.")]
        public string Instruction { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        public Guid CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Order must be a positive number.")]
        public int Order { get; set; }
    }
}
