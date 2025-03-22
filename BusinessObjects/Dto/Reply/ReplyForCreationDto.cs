using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Reply
{
    public class ReplyForCreationDto
    {
        [Required(ErrorMessage = "Review ID is required.")]
        public Guid ReviewId { get; set; }

        [Required(ErrorMessage = "Reply content is required.")]
        [StringLength(1000, ErrorMessage = "Reply content cannot exceed 1000 characters.")]
        public string ReplyContent { get; set; }
    }
}
