using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Review
{
    public class ReviewForUpdateDto
    {
        public List<string> ReviewImages { get; set; } = new List<string>();

        [Required(ErrorMessage = "Rating value is required.")]
        [Range(0, 5, ErrorMessage = "Rating value must be between 0 and 5.")]
        public float RatingValue { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string Comment { get; set; }
    }
}
