namespace BusinessObjects.Models.Dto.Dashboard;

public class TopProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}