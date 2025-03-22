using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.CancelReason
{
    public class CancelReasonForUpdateDto
    {
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(250, ErrorMessage = "Description can't exceed 250 characters.")]
        public string Description { get; set; } = null!;

        [Range(0, 1, ErrorMessage = "Refund rate must be between 0 and 1.")]
        public decimal RefundRate { get; set; }
    }
}
