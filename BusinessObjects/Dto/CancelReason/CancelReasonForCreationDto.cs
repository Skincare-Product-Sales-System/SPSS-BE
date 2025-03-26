using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.CancelReason
{
    public class CancelReasonForCreationDto
    {
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(250, ErrorMessage = "Description can't exceed 250 characters.")]
        public string Description { get; set; } = null!;

        [Range(0, 100, ErrorMessage = "Refund rate must be between 0 and 100.")]
        public decimal RefundRate { get; set; }
    }
}
