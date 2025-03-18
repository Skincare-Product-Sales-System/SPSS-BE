namespace BusinessObjects.Dto.Voucher;

public class VoucherDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }

    public double DiscountRate { get; set; }
    public int UsageLimit { get; set; }

    public double MinimumOrderValue { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }
    public string? CreatedBy { get; set; }

    public string? LastUpdatedBy { get; set; }

    public string? DeletedBy { get; set; }

    public DateTimeOffset? CreatedTime { get; set; }

    public DateTimeOffset? LastUpdatedTime { get; set; }

    public DateTimeOffset? DeletedTime { get; set; }

    public bool IsDeleted { get; set; }
}