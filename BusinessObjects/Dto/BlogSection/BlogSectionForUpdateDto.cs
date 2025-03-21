using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.BlogSection
{
    public class BlogSectionForUpdateDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Content type is required.")]
        [StringLength(50, ErrorMessage = "Content type can't exceed 50 characters.")]
        public string ContentType { get; set; }

        [StringLength(100, ErrorMessage = "Subtitle can't exceed 100 characters.")]
        public string Subtitle { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Order must be a positive integer.")]
        public int Order { get; set; }
    }
}
