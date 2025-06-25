using System;

namespace BusinessObjects.Models.Dto.Dashboard
{
    public class ProductProfitDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal ProcurementCost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
    }
}