using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Review
{
    public class ReviewForCreationDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public Guid ProductItemId { get; set; }

        [Range(0, 5, ErrorMessage = "Rating value must be between 0 and 5.")]
        public float RatingValue { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        public string CreatedBy { get; set; }
    }
}
