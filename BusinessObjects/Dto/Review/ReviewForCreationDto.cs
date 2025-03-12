using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Review
{
    public class ReviewForCreationDto
    {

        [Required]
        public Guid ProductItemId { get; set; }
        public List<string> ReviewImages { get; set; }

        [Range(0, 5, ErrorMessage = "Rating value must be between 0 and 5.")]
        public float RatingValue { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }
    }
}
