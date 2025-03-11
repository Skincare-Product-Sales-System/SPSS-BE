using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.ProductCategory
{
    public class ProductCategoryForUpdateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string CategoryName { get; set; }

        public Guid? ParentCategoryId { get; set; }

        [Required]
        public string LastUpdatedBy { get; set; }
    }
}
