using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Reply
{
    public class ReplyForCreationDto
    {
        [Required]
        public Guid ReviewId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Reply content cannot exceed 1000 characters.")]
        public string ReplyContent { get; set; }

        [Required]
        public string CreatedBy { get; set; }
    }
}
