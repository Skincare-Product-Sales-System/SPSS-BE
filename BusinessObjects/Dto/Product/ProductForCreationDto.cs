namespace BusinessObjects.Dto.Product;

public class ProductForCreationDto
{
    public Guid? ProductStatusId { get; set; }
    public Guid? BrandId { get; set; }

    public Guid? ProductCategoryId { get; set; }

    public string Name { get; set; }

    public string EnglishName { get; set; }

    public string Description { get; set; }

    public int SoldCount { get; set; }

    public double Rating { get; set; }

    public string StorageInstruction { get; set; }

    public string UsageInstruction { get; set; }

    public double VolumeWeight { get; set; }

    public string DetailedIngredients { get; set; }

    public string RegisterNumber { get; set; }

    public string MainFunction { get; set; }

    public string Texture { get; set; }

    public decimal Price { get; set; }

    public decimal MarketPrice { get; set; }

    public string KeyActiveIngredients { get; set; }

    public string ExpiryDate { get; set; }

    public string SkinIssues { get; set; }

    public string CreatedBy { get; set; }

    public string LastUpdatedBy { get; set; }

    public string DeletedBy { get; set; }

    public DateTimeOffset CreatedTime { get; set; }

    public DateTimeOffset? LastUpdatedTime { get; set; }

    public DateTimeOffset? DeletedTime { get; set; }

    public bool IsDeleted { get; set; }
}