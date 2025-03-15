namespace BusinessObjects.Dto.BlogSection
{
    public class BlogSectionForUpdateDto
    {
        public Guid? Id { get; set; }
        public string ContentType { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
    }
}
