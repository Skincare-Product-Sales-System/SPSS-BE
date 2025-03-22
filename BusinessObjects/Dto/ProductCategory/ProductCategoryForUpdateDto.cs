using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.ProductCategory
{
    public class ProductCategoryForUpdateDto
    {
        [Required(ErrorMessage = "Category ID is required.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(200, ErrorMessage = "Category name can't exceed 200 characters.")]
        public string CategoryName { get; set; }

        public Guid? ParentCategoryId { get; set; }

        [Required(ErrorMessage = "Last updated by is required.")]
        [StringLength(100, ErrorMessage = "Last updated by can't exceed 100 characters.")]
        public string LastUpdatedBy { get; set; }
    }
}
