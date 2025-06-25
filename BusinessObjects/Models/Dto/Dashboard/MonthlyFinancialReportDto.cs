using System;

namespace BusinessObjects.Models.Dto.Dashboard
{
    public class MonthlyFinancialReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public decimal ProcurementCost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int OrderCount { get; set; }
    }
}