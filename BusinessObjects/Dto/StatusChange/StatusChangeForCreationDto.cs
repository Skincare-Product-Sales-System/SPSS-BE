using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.StatusChange
{
    public class StatusChangeForCreationDto
    {
        [Required(ErrorMessage = "ID is required.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTimeOffset Date { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(100, ErrorMessage = "Status cannot exceed 100 characters.")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Order ID is required.")]
        public Guid OrderId { get; set; }
    }
}
