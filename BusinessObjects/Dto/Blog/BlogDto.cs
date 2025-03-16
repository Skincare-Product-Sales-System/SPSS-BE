using BusinessObjects.Dto.BlogSection;

namespace BusinessObjects.Dto.Blog;

public class BlogDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Thumbnail { get; set; }
    public DateTimeOffset? LastUpdatedTime { get; set; }
}