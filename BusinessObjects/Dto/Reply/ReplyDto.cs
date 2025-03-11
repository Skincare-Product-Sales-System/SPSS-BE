using System;

namespace BusinessObjects.Dto.Reply
{
    public class ReplyDto
    {
        public Guid Id { get; set; }
        public int? UserId { get; set; }
        public Guid ReviewId { get; set; }
        public string ReplyContent { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }
}
