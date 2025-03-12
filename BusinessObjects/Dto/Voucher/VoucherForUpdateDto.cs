namespace BusinessObjects.Dto.Voucher;

public class VoucherForUpdateDto
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }

    public double DiscountRate { get; set; }
    public string UsageLimit { get; set; }

    public double MinimumOrderValue { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }
}