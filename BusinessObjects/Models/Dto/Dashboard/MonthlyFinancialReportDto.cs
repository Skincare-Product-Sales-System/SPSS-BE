using System;

namespace BusinessObjects.Models.Dto.Dashboard
{
    public class MonthlyFinancialReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        
        // New properties for voucher analysis
        public decimal GrossRevenue { get; set; }     // Revenue before voucher discounts
        public decimal DiscountAmount { get; set; }   // Total amount of voucher discounts
        
        // Existing properties
        public decimal Revenue { get; set; }          // Net revenue after discounts
        public decimal ProcurementCost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int OrderCount { get; set; }
    }
}