using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Voucher;

public class VoucherForCreationDto
{
    [Required(ErrorMessage = "Code is required.")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters.")]
    public string Code { get; set; }

    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
    public string Status { get; set; }

    [Range(0, 100, ErrorMessage = "Discount rate must be between 0 and 100.")]
    public double DiscountRate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be at least 1.")]
    public int UsageLimit { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Minimum order value must be a positive number.")]
    public double MinimumOrderValue { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    public DateTimeOffset StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    public DateTimeOffset EndDate { get; set; }
}
