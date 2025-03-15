using BusinessObjects.Dto.ProductCategory;

namespace BusinessObjects.Dto.Variation
{
    public class VariationDto
    {
        public Guid Id { get; set; }
        public Guid? ProductCategoryId { get; set; }
        public string Name { get; set; }
        public ProductCategoryDto ProductCategory { get; set; }
    }
}
