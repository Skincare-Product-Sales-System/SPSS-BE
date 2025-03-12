namespace BusinessObjects.Dto.Brand;

public class BrandForCreationDto
{
    public int CountryId { get; set; }
    public string Name { get; set; } = null!;
    public string Title { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public bool? IsLiked { get; set; }
}