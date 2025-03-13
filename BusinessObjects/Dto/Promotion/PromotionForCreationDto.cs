namespace BusinessObjects.Dto.Promotion;

public class PromotionForCreationDto
{
    public string Name { get; set; }

    public string Type { get; set; }

    public string Description { get; set; }

    public decimal DiscountRate { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }
}