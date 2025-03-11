using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.ProductCategory
{
    public class ProductCategoryForCreationDto
    {
        [Required]
        public string CategoryName { get; set; }

        public Guid? ParentCategoryId { get; set; }

        [Required]
        public string CreatedBy { get; set; }
    }
}
