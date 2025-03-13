using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Review
{
    public class ReviewForUpdateDto
    {
        public List<string> ReviewImages { get; set; }

        [Required]
        public float RatingValue { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }
    }
}
