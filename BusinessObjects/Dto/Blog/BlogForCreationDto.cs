namespace BusinessObjects.Dto.Blog;

public class BlogForCreationDto
{
    public string Title { get; set; }
    public string Image { get; set; }
    public string BlogContent { get; set; }
    public Guid UserId { get; set; }
}