using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.VariationOption;

namespace BusinessObjects.Dto.Variation
{
    public class VariationDto
    {
        public Guid Id { get; set; }
        public CategoryForVariationQuery ProductCategory { get; set; }
        public List<VariationOptionForVariationQuery> VariationOptions { get; set; }
        public string Name { get; set; }
    }
}