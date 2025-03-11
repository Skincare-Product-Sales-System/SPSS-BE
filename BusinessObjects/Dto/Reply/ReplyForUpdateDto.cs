using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Reply
{
    public class ReplyForUpdateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid ReviewId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Reply content cannot exceed 1000 characters.")]
        public string ReplyContent { get; set; }

        [Required]
        public string LastUpdatedBy { get; set; }
    }
}
