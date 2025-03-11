using System;
using System.Collections.Generic;

namespace BusinessObjects.Dto.Review
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public Guid ProductItemId { get; set; }
        public float RatingValue { get; set; }
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public string DeletedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
