namespace BusinessObjects.Dto.ProductConfiguration
{
    public class ProductConfigurationForProductQueryDto
    {
        public string VariationName { get; set; } = null!;
        public string OptionName { get; set; } = null!;
        public Guid OptionId { get; set; }
    }
}
