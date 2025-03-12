using System;

namespace BusinessObjects.Dto.Reply
{
    public class ReplyDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string ReplyContent { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }
}
