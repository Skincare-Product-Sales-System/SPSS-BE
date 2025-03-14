using BusinessObjects.Dto.BlogImage;

namespace BusinessObjects.Dto.Blog
{
    public class BlogWithDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public string BlogContent { get; set; }
        public Guid UserId { get; set; }
        public List<string> BlogImages { get; set; }
    }
}
