using System;

namespace BusinessObjects.Dto.ProductCategory
{
    public class ProductCategoryDto
    {
        public Guid Id { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public string DeletedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
