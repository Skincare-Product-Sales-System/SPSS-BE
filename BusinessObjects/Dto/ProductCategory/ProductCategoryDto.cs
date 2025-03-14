using System;

namespace BusinessObjects.Dto.ProductCategory
{
    public class ProductCategoryDto
    {
        public Guid Id { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
