namespace BusinessObjects.Models
{
    public partial class BlogSection
    {
        public Guid Id { get; set; }
        public Guid BlogId { get; set; }
        public string ContentType { get; set; } // e.g., "text", "image", "video"
        public string Subtitle { get; set; } // Subtitle for text content
        public string Content { get; set; } // Stores actual content (text, URL for image, etc.)
        public int Order { get; set; } // To ensure proper ordering of sections
        public virtual Blog Blog { get; set; }
    }
}
