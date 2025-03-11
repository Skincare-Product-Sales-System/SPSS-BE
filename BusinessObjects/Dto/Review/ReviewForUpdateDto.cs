using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Review
{
    public class ReviewForUpdateDto
    {

        [Required]
        public Guid Id { get; set; }

        [Required]
        public float RatingValue { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        [Required]
        public string LastUpdatedBy { get; set; }
    }
}
