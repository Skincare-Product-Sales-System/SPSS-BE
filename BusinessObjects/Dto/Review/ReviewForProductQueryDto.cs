using BusinessObjects.Dto.Reply;

namespace BusinessObjects.Dto.Review
{
    public class ReviewForProductQueryDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public List<string> ReviewImages { get; set; }
        public List<string> VariationOptionValues { get; set; }
        public float RatingValue { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        public ReplyDto Reply { get; set; }
    }
}
