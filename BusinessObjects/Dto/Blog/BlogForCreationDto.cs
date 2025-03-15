using BusinessObjects.Dto.BlogSection;

namespace BusinessObjects.Dto.Blog;

public class BlogForCreationDto
{
    public string Title { get; set; }
    public string Thumbnail { get; set; } // URL for thumbnail/featured image
    public string Description { get; set; }
    public Guid UserId { get; set; }
    public List<BlogSectionForCreationDto> Sections { get; set; }
}