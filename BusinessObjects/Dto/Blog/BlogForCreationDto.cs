namespace BusinessObjects.Dto.Blog;

public class BlogForCreationDto
{
    public string Title { get; set; }
    public string Image { get; set; }
    public string BlogContent { get; set; }
    public List<string> BlogImages { get; set; }
}