namespace BusinessObjects.Dto.PromotionTarget;

public class PromotionTargetForCreationDto
{
    public Guid PromotionId { get; set; }

    public Guid? BrandId { get; set; }

    public Guid? ProductCategoryId { get; set; }

    public Guid? ProductId { get; set; }
}