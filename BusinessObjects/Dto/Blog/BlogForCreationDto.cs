using BusinessObjects.Dto.BlogSection;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Blog;

public class BlogForCreationDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title can't exceed 150 characters.")]
    public string Title { get; set; }

    [Url(ErrorMessage = "Invalid URL format for Thumbnail.")]
    public string Thumbnail { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "At least one section is required.")]
    public List<BlogSectionForCreationDto> Sections { get; set; }
}