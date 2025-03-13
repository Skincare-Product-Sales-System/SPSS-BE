using BusinessObjects.Dto.Reply;
using System;
using System.Collections.Generic;

namespace BusinessObjects.Dto.Review
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public string ProductImage { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public List<string> ReviewImages { get; set; }
        public List<string> VariationOptionValues { get; set; }
        public float RatingValue { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        public ReplyDto Reply { get; set; }
    }
}
