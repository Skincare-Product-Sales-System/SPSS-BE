using BusinessObjects.Dto.BlogSection;

namespace BusinessObjects.Dto.Blog;

public class BlogForUpdateDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Thumbnail { get; set; }
    public List<BlogSectionForUpdateDto> Sections { get; set; }
}
