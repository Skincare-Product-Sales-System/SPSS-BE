using System;

namespace BusinessObjects.Dto.ProductCategory
{
    public class ProductCategoryDto
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; }
        public Guid? ParentId { get; set; }
        public List<ProductCategoryDto> Children { get; set; } = new List<ProductCategoryDto>();
    }

}
