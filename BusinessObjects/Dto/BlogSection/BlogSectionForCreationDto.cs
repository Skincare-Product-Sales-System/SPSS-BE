namespace BusinessObjects.Dto.BlogSection
{
    public class BlogSectionForCreationDto
    {
        public string ContentType { get; set; } // e.g., "text", "image"
        public string Subtitle { get; set; } // Subtitle for text content
        public string Content { get; set; } // Text content or image URL
        public int Order { get; set; } // Section order
    }
}
