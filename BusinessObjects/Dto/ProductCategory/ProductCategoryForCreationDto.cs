using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.ProductCategory
{
    public class ProductCategoryForCreationDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(200, ErrorMessage = "Category name can't exceed 200 characters.")]
        public string CategoryName { get; set; }

        public Guid? ParentCategoryId { get; set; }
    }
}
